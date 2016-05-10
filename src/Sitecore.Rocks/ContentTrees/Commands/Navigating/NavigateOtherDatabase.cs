// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.UI;

namespace Sitecore.Rocks.ContentTrees.Commands.Navigating
{
    [Command(Submenu = "Navigate"), Feature(FeatureNames.AdvancedNavigation)]
    public class NavigateOtherDatabase : CommandBase
    {
        public NavigateOtherDatabase()
        {
            Text = Resources.NavigateOtherDatabase_NavigateOtherDatabase_Same_Item_in_Other_Database;
            Group = "OtherDatabase";
            SortingValue = 5000;

            SubmenuOpened = Opened;
        }

        [CanBeNull]
        protected ContentTreeContext Context { get; set; }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return false;
            }

            var item = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (item == null)
            {
                return false;
            }

            if (ActiveContext.ActiveContentTree == null)
            {
                return false;
            }

            Context = context;

            return true;
        }

        public override void Execute(object parameter)
        {
        }

        private void NavigateItem([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var contentTree = ActiveContext.ActiveContentTree;
            if (contentTree == null)
            {
                return;
            }

            var menuItem = e.Source as MenuItem;
            if (menuItem == null)
            {
                return;
            }

            var itemUri = menuItem.Tag as ItemUri;
            if (itemUri == null)
            {
                return;
            }

            contentTree.Locate(itemUri);
        }

        private void Opened([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var item = Context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (item == null)
            {
                return;
            }

            var menuItem = e.Source as MenuItem;
            if (menuItem == null)
            {
                return;
            }

            menuItem.Items.Clear();

            BaseTreeViewItem siteItem = item;
            while (siteItem != null && !(siteItem is SiteTreeViewItem))
            {
                siteItem = siteItem.Parent as BaseTreeViewItem;
            }

            if (siteItem == null)
            {
                return;
            }

            var databaseName = item.ItemUri.DatabaseName.ToString();

            foreach (var child in siteItem.Items)
            {
                var databaseItem = child as DatabaseTreeViewItem;
                if (databaseItem == null)
                {
                    continue;
                }

                var name = databaseItem.DatabaseUri.DatabaseName.ToString();
                if (name == databaseName)
                {
                    continue;
                }

                var m = new MenuItem
                {
                    Header = name,
                    Tag = new ItemUri(databaseItem.DatabaseUri, item.ItemUri.ItemId)
                };

                m.Click += NavigateItem;

                menuItem.Items.Add(m);
            }
        }
    }
}
