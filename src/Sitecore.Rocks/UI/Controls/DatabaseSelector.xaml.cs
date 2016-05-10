// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites.Connections;

namespace Sitecore.Rocks.UI.Controls
{
    public partial class DatabaseSelector
    {
        private DatabaseUri _databaseUri;

        private bool _isLoaded;

        public DatabaseSelector()
        {
            InitializeComponent();

            DatabaseUri = AppHost.Settings.ActiveDatabaseUri;
        }

        [NotNull]
        public DatabaseUri DatabaseUri
        {
            get { return _databaseUri; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                if (value == _databaseUri)
                {
                    return;
                }

                _databaseUri = value;

                if (DatabaseUri == DatabaseUri.Empty)
                {
                    DatabaseTextBlock.Text = "<None>";
                }
                else
                {
                    DatabaseTextBlock.Text = string.Format("{0} - {1}", DatabaseUri.Site.Name, DatabaseUri.DatabaseName);
                }

                var selectionChanged = SelectionChanged;
                if (selectionChanged != null)
                {
                    selectionChanged(this, DatabaseUri);
                }
            }
        }

        public event Notifications.DatabaseEventHandler SelectionChanged;

        private void GetChildren(object sender, BaseTreeViewItem baseTreeViewItem, List<BaseTreeViewItem> children)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(baseTreeViewItem, nameof(baseTreeViewItem));
            Debug.ArgumentNotNull(children, nameof(children));

            var siteTreeViewItem = baseTreeViewItem as SiteTreeViewItem;
            if (siteTreeViewItem != null)
            {
                for (var index = children.Count - 1; index >= 0; index--)
                {
                    var child = children[index];

                    if (!(child is DatabaseTreeViewItem))
                    {
                        children.Remove(child);
                    }
                }
            }

            var databaseTreeViewItem = baseTreeViewItem as DatabaseTreeViewItem;
            if (databaseTreeViewItem != null)
            {
                children.Clear();
            }
        }

        private void HandleKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key == Key.Escape)
            {
                Popup.IsOpen = false;
                e.Handled = true;
            }
        }

        private void LoadTreeView([NotNull] object sender, [NotNull] RoutedEventArgs routedEventArgs)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(routedEventArgs, nameof(routedEventArgs));

            TreeView.Loaded -= LoadTreeView;

            TreeView.Items.Clear();

            var folder = ConnectionManager.GetConnectionFolder();

            var item = new ConnectionFolderTreeViewItem(folder);

            TreeView.Items.Add(item);
            item.MakeExpandable();
            item.IsExpanded = true;

            _isLoaded = true;

            if (DatabaseUri == DatabaseUri.Empty)
            {
                return;
            }

            var itemUri = new ItemUri(DatabaseUri, ItemId.Empty);
            var databaseTreeViewItem = TreeView.FindDatabaseTreeViewItem(itemUri);
            if (databaseTreeViewItem == null)
            {
                return;
            }

            databaseTreeViewItem.BringIntoView();
            databaseTreeViewItem.IsSelected = true;
            databaseTreeViewItem.IsItemSelected = true;
            Keyboard.Focus(databaseTreeViewItem);
        }

        private void OpenDropDown([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!_isLoaded)
            {
                TreeView.Loaded += LoadTreeView;
            }

            Popup.IsOpen = true;
        }

        private void SetSelection([NotNull] object sender, [NotNull] RoutedPropertyChangedEventArgs<object> e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var item = TreeView.SelectedItem as DatabaseTreeViewItem;
            if (item == null)
            {
                return;
            }

            DatabaseUri = item.DatabaseUri;
            Popup.IsOpen = false;
        }
    }
}
