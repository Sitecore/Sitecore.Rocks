// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications.Storages;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ListViewExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.UI.Dialogs.SelectTemplatesDialogs
{
    [UsedImplicitly]
    public partial class TemplatePicker
    {
        public const string RegistryKey = "TemplatePicker";

        private readonly ListViewSorter listViewSorter;

        [CanBeNull]
        private CollectionView view;

        public TemplatePicker()
        {
            InitializeComponent();

            listViewSorter = new ListViewSorter(TemplateListView);

            Loaded += ControlLoaded;
        }

        [NotNull]
        public DatabaseUri DatabaseUri { get; set; }

        public bool IncludeBranches { get; private set; }

        [NotNull]
        public List<ItemId> SelectedItems { get; set; }

        public bool SetInitialFocus { get; set; } = true;

        [CanBeNull]
        public List<CheckedTemplateHeader> Templates { get; private set; }

        public void FocusItem([NotNull] ItemId itemId)
        {
            Assert.ArgumentNotNull(itemId, nameof(itemId));

            var templates = TemplateListView.Items.OfType<CheckedTemplateHeader>();

            var result = templates.FirstOrDefault(header => header.TemplateId == itemId);

            TemplateListView.SelectedItem = result;

            if (result != null)
            {
                TemplateListView.ScrollIntoView(result);
            }
        }

        [CanBeNull]
        public TemplateHeader GetTemplateHeader([NotNull] ItemId itemId)
        {
            Assert.ArgumentNotNull(itemId, nameof(itemId));

            if (Templates == null)
            {
                return null;
            }

            return Templates.FirstOrDefault(header => header.TemplateId == itemId);
        }

        public void Initialize([NotNull] DatabaseUri databaseUri, [NotNull] IEnumerable<ItemId> selectedItems, bool includeBranches)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(selectedItems, nameof(selectedItems));

            DatabaseUri = databaseUri;
            SelectedItems = new List<ItemId>(selectedItems);
            IncludeBranches = includeBranches;
        }

        public void Remove([NotNull] ItemId itemId)
        {
            Assert.ArgumentNotNull(itemId, nameof(itemId));

            if (Templates == null)
            {
                return;
            }

            foreach (var child in Templates)
            {
                if (child.TemplateId == itemId)
                {
                    child.IsChecked = false;

                    // workaround for filtered items
                    SelectedItems.Remove(child.TemplateId);
                }
            }
        }

        public event EventHandler SelectionChanged;

        public event EventHandler TemplatesLoaded;

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            AppHost.Server.GetTemplates(DatabaseUri, LoadTemplates, IncludeBranches);

            if (SetInitialFocus)
            {
                Keyboard.Focus(TemplateSelectorFilter.TextBox);
            }
        }

        private void FilterTextChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (view != null)
            {
                view.Refresh();
            }
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

        private void LoadTemplates([NotNull] IEnumerable<TemplateHeader> templates)
        {
            Debug.ArgumentNotNull(templates, nameof(templates));

            Templates = templates.Select(t => new CheckedTemplateHeader(t.TemplateUri, t.Name, t.Icon, t.Path, t.Section, t.IsBranch, SelectedItems.Contains(t.TemplateUri.ItemId))).ToList();

            TemplateListView.ItemsSource = Templates;

            listViewSorter.Resort();
            view = CollectionViewSource.GetDefaultView(Templates) as CollectionView;
            if (view == null)
            {
                return;
            }

            var groupDescription = new PropertyGroupDescription("Section")
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
                var templateHeader = o as TemplateHeader;
                return templateHeader != null && templateHeader.Name.IsFilterMatch(TemplateSelectorFilter.Text);
            };

            view.Refresh();

            TemplateListView.ResizeColumn(NameColumn);

            Loading.Visibility = Visibility.Collapsed;
            TemplateListView.Visibility = Visibility.Visible;

            var loaded = TemplatesLoaded;
            if (loaded != null)
            {
                loaded(this, EventArgs.Empty);
            }
        }

        private void OpenContextMenu(object sender, ContextMenuEventArgs e)
        {
            var selectedItem = TemplateListView.SelectedItem as CheckedTemplateHeader;
            if (selectedItem == null)
            {
                return;
            }

            var context = new ItemSelectionContext(new TemplatedItemDescriptor(selectedItem.TemplateUri, string.Empty, selectedItem.TemplateId, selectedItem.Name));

            TemplateGrid.ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void RaiseSelectionChanged()
        {
            var changed = SelectionChanged;
            if (changed != null)
            {
                changed(this, EventArgs.Empty);
            }
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

        private void TemplateCheck([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var checkBox = sender as CheckBox;
            if (checkBox == null)
            {
                return;
            }

            var templateHeader = checkBox.Tag as TemplateHeader;
            if (templateHeader == null)
            {
                return;
            }

            SelectedItems.Add(templateHeader.TemplateId);

            RaiseSelectionChanged();
        }

        private void TemplateUncheck([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var checkBox = sender as CheckBox;
            if (checkBox == null)
            {
                return;
            }

            var templateHeader = checkBox.Tag as TemplateHeader;
            if (templateHeader == null)
            {
                return;
            }

            SelectedItems.Remove(templateHeader.TemplateId);

            RaiseSelectionChanged();
        }

        public class CheckedTemplateHeader : TemplateHeader, INotifyPropertyChanged
        {
            private bool isChecked;

            public CheckedTemplateHeader([NotNull] ItemUri templateUri, [NotNull] string name, [NotNull] string icon, [NotNull] string path, [NotNull] string section, bool isBranch, bool isChecked) : base(templateUri, name, icon, path, section, isBranch)
            {
                Assert.ArgumentNotNull(templateUri, nameof(templateUri));
                Assert.ArgumentNotNull(name, nameof(name));
                Assert.ArgumentNotNull(icon, nameof(icon));
                Assert.ArgumentNotNull(path, nameof(path));
                Assert.ArgumentNotNull(section, nameof(section));

                IsChecked = isChecked;
            }

            public bool IsChecked
            {
                get { return isChecked; }

                set
                {
                    if (value.Equals(isChecked))
                    {
                        return;
                    }

                    isChecked = value;
                    OnPropertyChanged1("IsChecked");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged1([NotNull] string propertyName)
            {
                Debug.ArgumentNotNull(propertyName, nameof(propertyName));
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
    }
}
