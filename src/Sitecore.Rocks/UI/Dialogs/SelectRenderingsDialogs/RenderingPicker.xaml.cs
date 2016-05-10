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
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ListViewExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.UI.Dialogs.SelectRenderingsDialogs
{
    [UsedImplicitly]
    public partial class RenderingPicker
    {
        public const string RegistryKey = "RenderingPicker";

        private readonly ListViewSorter _listViewSorter;

        [CanBeNull]
        private CollectionView _view;

        public RenderingPicker()
        {
            InitializeComponent();

            _listViewSorter = new ListViewSorter(RenderingListView);

            Loaded += ControlLoaded;
        }

        [NotNull]
        public DatabaseUri DatabaseUri { get; set; }

        [CanBeNull]
        public List<CheckedItemHeader> Renderings { get; private set; }

        [NotNull]
        public List<ItemId> SelectedItems { get; set; }

        public void FocusItem([NotNull] ItemId itemId)
        {
            Assert.ArgumentNotNull(itemId, nameof(itemId));

            var renderings = RenderingListView.Items.OfType<CheckedItemHeader>();

            var result = renderings.FirstOrDefault(header => header.ItemId == itemId);

            RenderingListView.SelectedItem = result;

            if (result != null)
            {
                RenderingListView.ScrollIntoView(result);
            }
        }

        [CanBeNull]
        public ItemHeader GetItemHeader([NotNull] ItemId itemId)
        {
            Assert.ArgumentNotNull(itemId, nameof(itemId));

            if (Renderings == null)
            {
                return null;
            }

            return Renderings.FirstOrDefault(header => header.ItemId == itemId);
        }

        public void Initialize([NotNull] DatabaseUri databaseUri, [NotNull] IEnumerable<ItemId> selectedItems)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(selectedItems, nameof(selectedItems));

            DatabaseUri = databaseUri;
            SelectedItems = new List<ItemId>(selectedItems);
        }

        public void Remove([NotNull] ItemId itemId)
        {
            Assert.ArgumentNotNull(itemId, nameof(itemId));

            if (Renderings == null)
            {
                return;
            }

            foreach (var child in Renderings)
            {
                if (child.ItemId == itemId)
                {
                    child.IsChecked = false;

                    // workaround for filtered items
                    SelectedItems.Remove(child.ItemId);
                }
            }
        }

        public event EventHandler RenderingsLoaded;

        public event EventHandler SelectionChanged;

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            AppHost.Server.Layouts.GetRenderings(DatabaseUri, LoadRenderings);

            Keyboard.Focus(RenderinSelectorFilter.TextBox);
        }

        private void FilterTextChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (_view != null)
            {
                _view.Refresh();
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

        private void LoadRenderings([NotNull] string response, [NotNull] ExecuteResult executeResult)
        {
            if (!DataService.HandleExecute(response, executeResult))
            {
                return;
            }

            var root = response.ToXElement();
            if (root == null)
            {
                return;
            }

            var renderings = root.Elements().Select(element => ItemHeader.Parse(DatabaseUri, element)).ToList();

            Renderings = renderings.Select(t => new CheckedItemHeader(t, SelectedItems.Contains(t.ItemUri.ItemId))).ToList();

            RenderingListView.ItemsSource = Renderings;

            _listViewSorter.Resort();
            _view = CollectionViewSource.GetDefaultView(Renderings) as CollectionView;
            if (_view == null)
            {
                return;
            }

            var groupDescription = new PropertyGroupDescription("ParentPath")
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
                var itemHeader = o as ItemHeader;
                return itemHeader != null && itemHeader.Name.IsFilterMatch(RenderinSelectorFilter.Text);
            };

            _view.Refresh();

            RenderingListView.ResizeColumn(NameColumn);

            Loading.Visibility = Visibility.Collapsed;
            RenderingListView.Visibility = Visibility.Visible;

            var loaded = RenderingsLoaded;
            if (loaded != null)
            {
                loaded(this, EventArgs.Empty);
            }
        }

        private void OpenContextMenu(object sender, ContextMenuEventArgs e)
        {
            var selectedItem = RenderingListView.SelectedItem as CheckedItemHeader;
            if (selectedItem == null)
            {
                return;
            }

            var context = new ItemSelectionContext(new ItemDescriptor(selectedItem.ItemUri, selectedItem.Name));

            RenderingGrid.ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void RaiseSelectionChanged()
        {
            var changed = SelectionChanged;
            if (changed != null)
            {
                changed(this, EventArgs.Empty);
            }
        }

        private void RenderingCheck([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var checkBox = sender as CheckBox;
            if (checkBox == null)
            {
                return;
            }

            var itemHeader = checkBox.Tag as ItemHeader;
            if (itemHeader == null)
            {
                return;
            }

            SelectedItems.Add(itemHeader.ItemId);

            RaiseSelectionChanged();
        }

        private void RenderingUncheck([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var checkBox = sender as CheckBox;
            if (checkBox == null)
            {
                return;
            }

            var itemHeader = checkBox.Tag as ItemHeader;
            if (itemHeader == null)
            {
                return;
            }

            SelectedItems.Remove(itemHeader.ItemId);

            RaiseSelectionChanged();
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

        public class CheckedItemHeader : ItemHeader, INotifyPropertyChanged
        {
            private bool isChecked;

            public CheckedItemHeader([NotNull] ItemHeader itemHeader, bool isChecked)
            {
                ItemUri = itemHeader.ItemUri;
                Name = itemHeader.Name;
                Path = itemHeader.Path;
                Icon = itemHeader.Icon;
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
                    OnPropertyChanged("IsChecked");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([NotNull] string propertyName)
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
