// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.Sites.Connections;

namespace Sitecore.Rocks.UI.Dialogs.SelectDatabaseDialogs
{
    public partial class SelectDatabaseDialog
    {
        private bool isLoaded;

        private DatabaseUri selectedDatabaseUri;

        public SelectDatabaseDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            SelectedDatabaseUri = AppHost.Settings.ActiveDatabaseUri;
        }

        [NotNull]
        public DatabaseUri SelectedDatabaseUri
        {
            get { return selectedDatabaseUri ?? DatabaseUri.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                selectedDatabaseUri = value;
            }
        }

        [Obsolete]
        public void Initialize([NotNull] DatabaseUri selectedDatabaseUri)
        {
            Assert.ArgumentNotNull(selectedDatabaseUri, nameof(selectedDatabaseUri));

            SelectedDatabaseUri = selectedDatabaseUri;

            Load();
        }

        [Obsolete]
        public void Initialize([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            SelectedDatabaseUri = new DatabaseUri(site, new DatabaseName("master"));

            Load();
        }

        public new bool ShowDialog()
        {
            Load();

            return AppHost.Shell.ShowDialog(this) == true;
        }

        private void EnableButtons([NotNull] object sender, [NotNull] RoutedPropertyChangedEventArgs<object> e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
        }

        private void EnableButtons()
        {
            OkButton.IsEnabled = TreeView.SelectedItem is DatabaseTreeViewItem;
        }

        private void GetChildren([NotNull] object sender, [NotNull] BaseTreeViewItem baseTreeViewItem, [NotNull] List<BaseTreeViewItem> children)
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
                e.Handled = true;
                this.Close(false);
                return;
            }

            if (e.Key == Key.Enter)
            {
                OkClicked(sender, e);
                e.Handled = true;
            }
        }

        private void HandleMouseDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var databaseTreeViewItem = TreeView.SelectedItem as DatabaseTreeViewItem;
            if (databaseTreeViewItem != null)
            {
                OkClicked(sender, e);
                e.Handled = true;
            }
        }

        private void Load()
        {
            if (isLoaded)
            {
                return;
            }

            isLoaded = true;

            if (SelectedDatabaseUri == DatabaseUri.Empty)
            {
                SelectedDatabaseUri = AppHost.Settings.ActiveDatabaseUri;
            }

            LoadDatabases();
            EnableButtons();
        }

        private void LoadDatabases()
        {
            TreeView.Items.Clear();

            var folder = ConnectionManager.GetConnectionFolder();

            var item = new ConnectionFolderTreeViewItem(folder);

            item.MakeExpandable();

            TreeView.Items.Add(item);

            item.ExpandAndWait();

            if (SelectedDatabaseUri == DatabaseUri.Empty)
            {
                return;
            }

            LocateDatabase();
        }

        private void LocateDatabase()
        {
            var itemUri = new ItemUri(SelectedDatabaseUri, ItemId.Empty);
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

        private void OkClicked([NotNull] object sender, [NotNull] RoutedEventArgs routedEventArgs)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(routedEventArgs, nameof(routedEventArgs));

            var databaseTreeViewItem = TreeView.SelectedItem as DatabaseTreeViewItem;
            if (databaseTreeViewItem == null)
            {
                AppHost.MessageBox("Please select a database.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SelectedDatabaseUri = databaseTreeViewItem.DatabaseUri;

            this.Close(true);
        }
    }
}
