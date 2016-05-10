// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Favorites;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.VirtualItems.Favorites
{
    public class FavoriteRootTreeViewItem : BaseTreeViewItem, ICanDrop
    {
        private const string RegistryKey = "Expanded";

        private const string RegistryPath = "ContentTree\\Favorites";

        public FavoriteRootTreeViewItem()
        {
            Text = Rocks.Resources.Favorites;
            Margin = new Thickness(0, 2, 0, 0);
            Icon = new Icon("Resources/16x16/star_yellow.png");
            ToolTip = "Favorite items";

            Notifications.RegisterItemEvents(this, deleted: ItemDeleted, renamed: ItemRenamed);

            Loaded += ControlLoaded;
            Expanded += SetExpanderState;
            Collapsed += SetExpanderState;
        }

        public void AddFavorites([NotNull] IEnumerable<IItem> baseItems, int index)
        {
            Assert.ArgumentNotNull(baseItems, nameof(baseItems));

            if (Items.Count == 1 && Items[0] == DummyTreeViewItem.Instance)
            {
                Expand(false);
            }

            var favorites = FavoriteManager.GetFavorites();

            if (index < 0 || index > favorites.Count())
            {
                index = favorites.Count();
            }

            foreach (var item in baseItems)
            {
                var fullPath = item.Name;
                var itemTreeViewItem = item as ItemTreeViewItem;
                if (itemTreeViewItem != null)
                {
                    fullPath = itemTreeViewItem.GetPath();
                }

                var favorite = new Favorite
                {
                    Name = item.Name,
                    ItemVersionUri = new ItemVersionUri(item.ItemUri, LanguageManager.CurrentLanguage, Version.Latest),
                    FullPath = fullPath,
                    Icon = item.Icon
                };

                favorites.Insert(index, favorite);

                var favoriteTreeViewItem = new FavoriteTreeViewItem
                {
                    Text = favorite.Name
                };

                favoriteTreeViewItem.Initialize(favorite);

                Items.Insert(index, favoriteTreeViewItem);
            }

            FavoriteManager.Save();
        }

        public override bool GetChildren(GetChildrenDelegate callback, bool async)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            var result = new List<BaseTreeViewItem>();

            var favorites = FavoriteManager.GetFavorites();

            foreach (var favorite in favorites)
            {
                var favoriteTreeViewItem = new FavoriteTreeViewItem
                {
                    Text = favorite.Name
                };

                favoriteTreeViewItem.Initialize(favorite);

                result.Add(favoriteTreeViewItem);
            }

            callback(result);

            return true;
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs routedEventArgs)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(routedEventArgs, nameof(routedEventArgs));

            Loaded -= ControlLoaded;

            IsExpanded = GetExpanderState();
        }

        private static bool GetExpanderState()
        {
            return (string)AppHost.Settings.Get(RegistryPath, RegistryKey, @"1") == @"1";
        }

        void ICanDrop.HandleDragOver(object sender, DragEventArgs e, BaseTreeViewItem treeViewItem)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));
            Debug.ArgumentNotNull(treeViewItem, nameof(treeViewItem));

            if (e.Data.GetDataPresent(DragManager.DragIdentifier))
            {
                e.Effects = DragDropEffects.Copy;
            }
        }

        void ICanDrop.HandleDrop(object sender, DragEventArgs e, BaseTreeViewItem treeViewItem)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));
            Debug.ArgumentNotNull(treeViewItem, nameof(treeViewItem));

            if (!e.Data.GetDataPresent(DragManager.DragIdentifier))
            {
                return;
            }

            var baseItems = e.Data.GetData(DragManager.DragIdentifier) as IEnumerable<IItem>;
            if (baseItems != null)
            {
                AddFavorites(baseItems, -1);
            }
        }

        private void ItemDeleted([NotNull] object sender, [NotNull] ItemUri itemuri)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(itemuri, nameof(itemuri));

            for (var n = Items.Count - 1; n >= 0; n--)
            {
                var item = Items[n];

                var favorite = item as FavoriteTreeViewItem;
                if (favorite == null)
                {
                    continue;
                }

                if (favorite.Favorite.ItemVersionUri.ItemUri == itemuri)
                {
                    Items.Remove(item);
                }
            }
        }

        private void ItemRenamed([NotNull] object sender, [NotNull] ItemUri itemuri, [NotNull] string newname)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(itemuri, nameof(itemuri));
            Debug.ArgumentNotNull(newname, nameof(newname));

            foreach (var item in Items)
            {
                var favorite = item as FavoriteTreeViewItem;
                if (favorite == null)
                {
                    continue;
                }

                if (favorite.Favorite.ItemVersionUri.ItemUri == itemuri)
                {
                    favorite.Text = newname;
                }
            }
        }

        private void SetExpanderState([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Settings.Set(RegistryPath, RegistryKey, IsExpanded ? @"1" : @"0");
            e.Handled = true;
        }
    }
}
