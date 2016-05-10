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
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications.Storages;
using Sitecore.Rocks.Controls.TemplateSelector.Filters;
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

namespace Sitecore.Rocks.Controls.TemplateSelector
{
    public partial class TemplateSelector
    {
        public const string RegistryKey = "TemplateSelector";

        private readonly ListViewSorter _listViewSorter;

        private readonly List<TemplateHeader> _templates = new List<TemplateHeader>();

        private DatabaseUri _databaseUri;

        [CanBeNull]
        private CollectionView _view;

        public TemplateSelector()
        {
            InitializeComponent();

            _listViewSorter = new ListViewSorter(TemplateListView);
            FilterTextBox.Text = AppHost.Settings.GetString(RegistryKey, "Filter", string.Empty);

            AppHost.Extensibility.ComposeParts(this);

            Loaded += ControlLoaded;
        }

        public bool AllowBranches { get; set; }

        [NotNull]
        public DatabaseUri DatabaseUri
        {
            get { return _databaseUri; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                _databaseUri = value;
            }
        }

        public bool IncludeBranches { get; set; }

        [CanBeNull]
        public ItemId InitialTemplateId { get; set; }

        [CanBeNull]
        public TemplateHeader SelectedTemplate
        {
            get { return TemplateListView.SelectedItem as TemplateHeader; }
        }

        [NotNull, ImportMany(typeof(ITemplateSelectorFilter))]
        protected List<ITemplateSelectorFilter> TemplateSelectorFilters { get; set; }

        public void AddToRecent([NotNull] TemplateHeader template)
        {
            Assert.ArgumentNotNull(template, nameof(template));

            foreach (var source in TemplateSelectorFilters)
            {
                source.AddToRecent(template);
            }
        }

        public event MouseButtonEventHandler DoubleClick;

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

            var customFilter = new CustomTemplateSelectorFilter(dialog.FilterName ?? string.Empty, dialog.FilterText ?? string.Empty, dialog.RootPath ?? string.Empty);
            customFilter.SetTemplates(_templates);

            TemplateSelectorFilters.Add(customFilter);

            SaveFilters();
            RenderFilters(customFilter.Name);
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            LoadFilters();
            RenderFilters();

            DatabaseUri.Site.DataService.ExecuteAsync("Templates.GetTemplates", SetTemplates, DatabaseUri.DatabaseName.ToString(), IncludeBranches ? "true" : "false");

            Keyboard.Focus(FilterTextBox.TextBox);
        }

        private void FilterTextChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (_view != null)
            {
                _view.Refresh();
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

            if (SelectedTemplate == null)
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

            _listViewSorter.HeaderClick(sender, e);
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

                var customFilter = new CustomTemplateSelectorFilter(name, filterText, rootPath);

                TemplateSelectorFilters.Add(customFilter);
            }
        }

        private void LoadTemplates([NotNull] IEnumerable<TemplateHeader> templates)
        {
            Debug.ArgumentNotNull(templates, nameof(templates));

            TemplateListView.ItemsSource = templates;

            _listViewSorter.Resort();
            _view = CollectionViewSource.GetDefaultView(templates) as CollectionView;
            if (_view == null)
            {
                return;
            }

            var groupDescription = new PropertyGroupDescription("Section")
            {
                StringComparison = StringComparison.InvariantCultureIgnoreCase
            };

            var collection = _view.GroupDescriptions;
            if (collection != null)
            {
                collection.Clear();
                collection.Add(groupDescription);
            }

            _view.Filter = delegate(object o)
            {
                var templateHeader = o as TemplateHeader;
                if (templateHeader == null)
                {
                    return false;
                }

                return templateHeader.Name.IsFilterMatch(FilterTextBox.Text);
            };

            _view.Refresh();

            TemplateListView.ResizeColumn(NameColumn);

            Loading.Visibility = Visibility.Collapsed;
            Stack.Visibility = Visibility.Visible;

            var selectedItem = templates.FirstOrDefault(t => t.TemplateId == InitialTemplateId);
            TemplateListView.SelectedItem = selectedItem;
            if (selectedItem != null)
            {
                TemplateListView.ScrollIntoView(selectedItem);
            }
        }

        [NotNull]
        private IEnumerable<TemplateHeader> ParseTemplates([NotNull] DatabaseUri databaseUri, [NotNull] XElement root)
        {
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Debug.ArgumentNotNull(root, nameof(root));

            var result = new List<TemplateHeader>();

            foreach (var child in root.Elements())
            {
                var itemId = new ItemId(new Guid(child.GetAttributeValue("id")));
                var itemUri = new ItemUri(databaseUri, itemId);

                var parentPath = AppHost.Files.GetDirectoryName(child.GetAttributeValue("path")).Replace("\\", "/");
                var template = new TemplateHeader(itemUri, child.Value, child.GetAttributeValue("icon"), child.GetAttributeValue("path"), parentPath, child.Name == "branch");

                result.Add(template);
            }

            return result;
        }

        private void RemoveFilter([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = FilterListBox.RemoveSelectedItem() as ListBoxItem;
            if (selectedItem == null)
            {
                return;
            }

            var filter = selectedItem.Tag as ITemplateSelectorFilter;
            if (filter == null)
            {
                return;
            }

            TemplateSelectorFilters.Remove(filter);
            SaveFilters();
        }

        private void RenderFilters([NotNull] string selectedTab = "")
        {
            Debug.ArgumentNotNull(selectedTab, nameof(selectedTab));

            FilterListBox.Items.Clear();

            if (string.IsNullOrEmpty(selectedTab))
            {
                selectedTab = AppHost.Settings.GetString("TemplateSelector", "SelectedTab", string.Empty);
            }

            ListBoxItem selectedItem = null;

            foreach (var filter in TemplateSelectorFilters.OrderBy(s => s.Priority).ThenBy(s => s.Name))
            {
                var item = new ListBoxItem
                {
                    Content = filter.Name,
                    Tag = filter
                };

                if (filter.Name == selectedTab)
                {
                    selectedItem = item;
                }

                FilterListBox.Items.Add(item);
            }

            FilterListBox.SelectedItem = selectedItem;
        }

        private void SaveFilters()
        {
            var writer = new StringWriter();
            var output = new OutputWriter(writer);

            output.WriteStartElement("filters");

            foreach (var filter in TemplateSelectorFilters.OfType<CustomTemplateSelectorFilter>())
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

        private void SetFilter([NotNull] object sender, [NotNull] SelectionChangedEventArgs selectionChangedEventArgs)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(selectionChangedEventArgs, nameof(selectionChangedEventArgs));

            RemoveFilterButton.IsEnabled = false;

            if (_templates.Count == 0)
            {
                return;
            }

            var listBoxItem = FilterListBox.SelectedItem as ListBoxItem;
            if (listBoxItem == null)
            {
                return;
            }

            var filter = listBoxItem.Tag as ITemplateSelectorFilter;
            if (filter == null)
            {
                return;
            }

            RemoveFilterButton.IsEnabled = filter is CustomTemplateSelectorFilter;
            Loading.Visibility = Visibility.Visible;
            Stack.Visibility = Visibility.Collapsed;

            var parameters = new TemplateSelectorFiltersParameters(DatabaseUri, IncludeBranches);

            filter.GetTemplates(parameters, LoadTemplates);

            AppHost.Settings.SetString("TemplateSelector", "SelectedTab", filter.Name);
        }

        private void SetTemplates([NotNull] string response, [NotNull] ExecuteResult executeResult)
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

            _templates.AddRange(ParseTemplates(DatabaseUri, root));

            foreach (var filter in TemplateSelectorFilters)
            {
                filter.SetTemplates(_templates);
            }

            var selectedItem = FilterListBox.SelectedItem as ListBoxItem;
            if (selectedItem != null)
            {
                selectedItem.IsSelected = false;
                selectedItem.IsSelected = true;
                return;
            }

            var firstItem = FilterListBox.Items.OfType<ListBoxItem>().FirstOrDefault();
            if (firstItem != null)
            {
                firstItem.IsSelected = true;
            }
        }

        private void TemplatesSelectionChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var changed = SelectionChanged;
            if (changed != null)
            {
                changed(sender, e);
            }
        }
    }
}
