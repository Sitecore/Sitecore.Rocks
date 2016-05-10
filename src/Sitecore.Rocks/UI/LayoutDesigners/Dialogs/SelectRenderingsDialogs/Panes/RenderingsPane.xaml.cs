// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications.Storages;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Extensions.ListBoxExtensions;
using Sitecore.Rocks.Extensions.ListViewExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.IO;
using Sitecore.Rocks.UI.LayoutDesigners.Dialogs.SelectRenderingsDialogs.Dialogs.AddFilterDialogs;
using Sitecore.Rocks.UI.LayoutDesigners.Dialogs.SelectRenderingsDialogs.Filters;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs.SelectRenderingsDialogs.Panes
{
    [Export(typeof(ISelectRenderingsDialogPane), Priority = 1000)]
    public partial class RenderingsPane : ISelectRenderingsDialogPane
    {
        public const string RegistryKey = "RenderingSelector";

        private readonly ListViewSorter listViewSorter;

        private readonly List<ItemHeader> renderings = new List<ItemHeader>();

        [CanBeNull]
        private CollectionView view;

        public RenderingsPane()
        {
            InitializeComponent();

            AllowMultipleRenderings = true;
            Header = "Renderings";

            FilterTextBox.Text = AppHost.Settings.GetString(RegistryKey, "Filter", string.Empty);
            listViewSorter = new ListViewSorter(RenderingsListView);

            AppHost.Extensibility.ComposeParts(this);

            Loaded += ControlLoaded;
        }

        public bool AllowMultipleRenderings { get; }

        public DatabaseUri DatabaseUri { get; set; }

        public string Header { get; }

        public IRenderingContainer RenderingContainer { get; set; }

        [CanBeNull]
        public ItemHeader SelectedRendering => RenderingsListView.SelectedItem as ItemHeader;

        public string SpeakCoreVersion { get; set; } = string.Empty;

        [NotNull, ImportMany(typeof(IRenderingSelectorFilter))]
        protected List<IRenderingSelectorFilter> RenderingSelectorFilters { get; set; }

        public bool AreButtonsEnabled()
        {
            return SelectedRendering != null;
        }

        public void Close()
        {
            if (SelectedRendering == null)
            {
                return;
            }

            foreach (var source in RenderingSelectorFilters)
            {
                source.AddToRecent(SelectedRendering);
            }
        }

        public event MouseButtonEventHandler DoubleClick;

        public void GetSelectedRenderings(Action<IEnumerable<RenderingItem>> completed)
        {
            Assert.ArgumentNotNull(completed, nameof(completed));

            if (SelectedRendering == null)
            {
                completed(Enumerable.Empty<RenderingItem>());
                return;
            }

            var result = new[]
            {
                new RenderingItem(RenderingContainer, SelectedRendering)
            };

            completed(result);
        }

        public event SelectionChangedEventHandler SelectionChanged;

        private void AddFilter([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var dialog = new AddFilterDialog
            {
                DatabaseUri = DatabaseUri
            };

            if (!dialog.ShowDialog())
            {
                return;
            }

            var customFilter = new CustomRenderingSelectorFilter(dialog.FilterName ?? string.Empty, dialog.FilterText ?? string.Empty, dialog.RootPath ?? string.Empty);
            customFilter.SetRenderings(renderings);

            RenderingSelectorFilters.Add(customFilter);

            SaveFilters();
            RenderFilters(customFilter.Name);
        }

        private void ChangeFilter([NotNull] object sender, [NotNull] SelectionChangedEventArgs selectionChangedEventArgs)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(selectionChangedEventArgs, nameof(selectionChangedEventArgs));

            selectionChangedEventArgs.Handled = true;

            RemoveFilterButton.IsEnabled = false;

            if (renderings.Count == 0)
            {
                return;
            }

            var listBoxItem = FilterListbox.SelectedItem as ListBoxItem;
            if (listBoxItem == null)
            {
                return;
            }

            var filter = listBoxItem.Tag as IRenderingSelectorFilter;
            if (filter == null)
            {
                return;
            }

            RemoveFilterButton.IsEnabled = filter is CustomRenderingSelectorFilter;
            Loading.Visibility = Visibility.Visible;
            Stack.Visibility = Visibility.Collapsed;

            var parameters = new RenderingSelectorFilterParameters(DatabaseUri);
            filter.GetRenderings(parameters, LoadRenderings);

            AppHost.Settings.SetString("RenderingSelector", "SelectedTab", filter.Name);
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            LoadFilters();
            RenderFilters();

            DatabaseUri.Site.DataService.ExecuteAsync("Layouts.GetRenderings", SetRenderings, DatabaseUri.DatabaseName.ToString());

            Keyboard.Focus(FilterTextBox.TextBox);
            FilterTextBox.TextBox.SelectAll();
        }

        private void FilterTextChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (view != null)
            {
                view.Refresh();
            }

            AppHost.Settings.SetString(RegistryKey, "Filter", FilterTextBox.Text);
        }

        [CanBeNull]
        private string GetGroupName([NotNull] object sender)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));

            var expander = sender as Expander;
            if (expander == null)
            {
                return null;
            }

            var group = expander.Tag as CollectionViewGroup;
            if (group == null)
            {
                return null;
            }

            return group.Name as string ?? string.Empty;
        }

        private void HandleMouseDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (SelectedRendering == null)
            {
                return;
            }

            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement != null)
            {
                if (frameworkElement.GetAncestorOrSelf<RepeatButton>() != null)
                {
                    return;
                }

                if (frameworkElement.GetAncestorOrSelf<GridViewColumnHeader>() != null)
                {
                    return;
                }

                if (frameworkElement.GetAncestorOrSelf<Thumb>() != null)
                {
                    return;
                }
            }

            var doubleClick = DoubleClick;
            if (doubleClick != null)
            {
                doubleClick(sender, e);
            }
        }

        private void HeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            listViewSorter.HeaderClick(sender, e);
        }

        private void InitExpander([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var expander = sender as Expander;
            if (expander == null)
            {
                return;
            }

            var name = GetGroupName(sender);
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            if (!Storage.ReadBool(RegistryKey + "\\Groups", name, true))
            {
                expander.IsExpanded = false;
            }
        }

        private void LoadFilters()
        {
            var filters = AppHost.Settings.GetString(RegistryKey, "Filters", string.Empty);
            if (string.IsNullOrEmpty(filters))
            {
                return;
            }

            var root = filters.ToXElement();
            if (root == null)
            {
                return;
            }

            foreach (var element in root.Elements())
            {
                var name = element.GetAttributeValue("name");
                var filterText = element.GetAttributeValue("filter");
                var rootPath = element.GetAttributeValue("root");

                var customFilter = new CustomRenderingSelectorFilter(name, filterText, rootPath);

                RenderingSelectorFilters.Add(customFilter);
            }
        }

        private void LoadRenderings([NotNull] IEnumerable<ItemHeader> items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            RenderingsListView.ItemsSource = items;

            listViewSorter.Resort();
            view = CollectionViewSource.GetDefaultView(items) as CollectionView;
            if (view == null)
            {
                return;
            }

            var groupDescription = new PropertyGroupDescription("ParentPath")
            {
                StringComparison = StringComparison.InvariantCultureIgnoreCase
            };

            var collection = view.GroupDescriptions;
            if (collection != null)
            {
                collection.Clear();
                collection.Add(groupDescription);
            }

            view.Filter = delegate(object o)
            {
                var itemHeader = o as ItemHeader;
                if (itemHeader == null)
                {
                    return false;
                }

                return itemHeader.Name.IsFilterMatch(FilterTextBox.Text);
            };

            view.Refresh();

            RenderingsListView.ResizeColumn(NameColumn);
            RenderingsListView.ResizeColumn(TemplateNameColumn);

            Loading.Visibility = Visibility.Collapsed;
            Stack.Visibility = Visibility.Visible;
        }

        private bool MatchesSpeakCoreVersion([NotNull] ItemHeader itemHeader)
        {
            Debug.ArgumentNotNull(itemHeader, nameof(itemHeader));

            var version = ((IItemData)itemHeader).GetData("ex.speakversionid") ?? string.Empty;

            return string.IsNullOrEmpty(version) || string.IsNullOrEmpty(SpeakCoreVersion) || string.Compare(version, SpeakCoreVersion, StringComparison.OrdinalIgnoreCase) == 0;
        }

        private void RemoveFilter([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = FilterListbox.RemoveSelectedItem() as ListBoxItem;
            if (selectedItem == null)
            {
                return;
            }

            var filter = selectedItem.Tag as IRenderingSelectorFilter;
            if (filter == null)
            {
                return;
            }

            RenderingSelectorFilters.Remove(filter);
            SaveFilters();
        }

        private void RenderFilters([NotNull] string selectedFilter = "")
        {
            Debug.ArgumentNotNull(selectedFilter, nameof(selectedFilter));

            FilterListbox.Items.Clear();

            if (string.IsNullOrEmpty(selectedFilter))
            {
                selectedFilter = AppHost.Settings.GetString("RenderingSelector", "SelectedTab", string.Empty);
            }

            ListBoxItem selectedItem = null;

            foreach (var filter in RenderingSelectorFilters.OrderBy(s => s.Priority).ThenBy(s => s.Name))
            {
                var item = new ListBoxItem
                {
                    Content = filter.Name,
                    Tag = filter
                };

                if (filter.Name == selectedFilter)
                {
                    selectedItem = item;
                }

                FilterListbox.Items.Add(item);
            }

            if (selectedItem != null)
            {
                selectedItem.IsSelected = true;
            }
        }

        private void RenderingsSelectionChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var changed = SelectionChanged;
            if (changed != null)
            {
                changed(sender, e);
            }
        }

        private void SaveFilters()
        {
            var writer = new StringWriter();
            var output = new OutputWriter(writer);

            output.WriteStartElement("filters");

            foreach (var filter in RenderingSelectorFilters.OfType<CustomRenderingSelectorFilter>())
            {
                output.WriteStartElement("filter");
                output.WriteAttributeString("name", filter.Name);

                if (!string.IsNullOrEmpty(filter.FilterText))
                {
                    output.WriteAttributeString("filter", filter.FilterText);
                }

                if (!string.IsNullOrEmpty(filter.RootPath))
                {
                    output.WriteAttributeString("root", filter.RootPath);
                }

                output.WriteEndElement();
            }

            output.WriteEndElement();

            AppHost.Settings.SetString(RegistryKey, "Filters", writer.ToString());
        }

        private void SetCollapsed([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Handled = true;

            var name = GetGroupName(sender);
            if (!string.IsNullOrEmpty(name))
            {
                Storage.WriteBool(RegistryKey + "\\Groups", name, false);
            }
        }

        private void SetExpanded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Handled = true;

            var name = GetGroupName(sender);
            if (!string.IsNullOrEmpty(name))
            {
                Storage.WriteBool(RegistryKey + "\\Groups", name, true);
            }
        }

        private void SetRenderings([NotNull] string response, [NotNull] ExecuteResult executeResult)
        {
            Debug.ArgumentNotNull(response, nameof(response));
            Debug.ArgumentNotNull(executeResult, nameof(executeResult));

            if (!DataService.HandleExecute(response, executeResult))
            {
                return;
            }

            var root = response.ToXElement();
            if (root == null)
            {
                return;
            }

            var items = root.Elements().Select(element => ItemHeader.Parse(DatabaseUri, element)).Where(MatchesSpeakCoreVersion).ToList();
            renderings.AddRange(items);

            foreach (var source in RenderingSelectorFilters)
            {
                source.SetRenderings(renderings);
            }

            var selectedItem = FilterListbox.SelectedItem as ListBoxItem;
            if (selectedItem != null)
            {
                selectedItem.IsSelected = false;
                selectedItem.IsSelected = true;
                return;
            }

            var firstItem = FilterListbox.Items.OfType<ListBoxItem>().FirstOrDefault();
            if (firstItem != null)
            {
                firstItem.IsSelected = true;
            }
        }
    }
}
