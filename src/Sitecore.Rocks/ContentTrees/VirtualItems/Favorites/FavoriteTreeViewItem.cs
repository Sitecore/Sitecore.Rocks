// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Favorites;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;

namespace Sitecore.Rocks.ContentTrees.VirtualItems.Favorites
{
    public class FavoriteTreeViewItem : BaseTreeViewItem, ICanDeleteWithText, IItem, ICanDrag, ICanDrop
    {
        private const string DragIdentifier = "SitecoreFavorites";

        private readonly ControlDragAdorner adorner;

        public FavoriteTreeViewItem()
        {
            Icon = new Icon("Resources/16x16/star_yellow.png");

            MouseDown += Locate;
            MouseDoubleClick += Open;
            ToolTipOpening += OpenToolTip;

            adorner = new ControlDragAdorner(ItemHeader, ControlDragAdornerPosition.All);
        }

        public string CommandText
        {
            get { return "Remove"; }
        }

        [NotNull]
        public Favorite Favorite { get; private set; }

        public ItemUri ItemUri
        {
            get { return Favorite.ItemVersionUri.ItemUri; }
        }

        public string MultipleText
        {
            get { return "Are you sure you want to remove these '{0}' favorites?"; }
        }

        public string SingleText
        {
            get { return "Are you sure you want to remove '{0}'?"; }
        }

        string IItem.Name
        {
            get { return Favorite.Name; }
        }

        public void Delete(bool deleteFiles)
        {
            FavoriteManager.Delete(Favorite);
            FavoriteManager.Save();

            var parent = Parent as TreeViewItem;
            if (parent != null)
            {
                parent.Items.Remove(this);
            }
        }

        public override bool GetChildren(GetChildrenDelegate callback, bool async)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            callback(Enumerable.Empty<BaseTreeViewItem>());

            return true;
        }

        public string GetDragIdentifier()
        {
            return DragIdentifier;
        }

        public void HandleDragOver(object sender, DragEventArgs e, BaseTreeViewItem treeViewItem)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));
            Assert.ArgumentNotNull(treeViewItem, nameof(treeViewItem));

            adorner.AllowedPositions = ControlDragAdornerPosition.None;

            if (e.Data.GetDataPresent(DragIdentifier))
            {
                e.Effects = DragDropEffects.Move;
                adorner.AllowedPositions = ControlDragAdornerPosition.Top | ControlDragAdornerPosition.Bottom;
            }

            if (!e.Data.GetDataPresent(DragManager.DragIdentifier))
            {
                return;
            }

            adorner.AllowedPositions = ControlDragAdornerPosition.Top | ControlDragAdornerPosition.Bottom;
            var h = adorner.GetHitTest(e);
            e.Effects = (h & adorner.AllowedPositions) != ControlDragAdornerPosition.None ? DragDropEffects.Move : DragDropEffects.None;
        }

        public void HandleDrop(object sender, DragEventArgs e, BaseTreeViewItem treeViewItem)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));
            Assert.ArgumentNotNull(treeViewItem, nameof(treeViewItem));

            e.Handled = true;

            if (e.Data.GetDataPresent(DragIdentifier))
            {
                var items = (List<BaseTreeViewItem>)e.Data.GetData(DragIdentifier);
                DropFavorites(items);
                return;
            }

            if (e.Data.GetDataPresent(DragManager.DragIdentifier))
            {
                var items = (IEnumerable<IItem>)e.Data.GetData(DragManager.DragIdentifier);
                DropItems(items);
            }
        }

        public void Initialize([NotNull] Favorite favorite)
        {
            Assert.ArgumentNotNull(favorite, nameof(favorite));

            Favorite = favorite;

            Icon = favorite.Icon;
            ToolTip = favorite.FullPath;
        }

        private void DropFavorites([NotNull] List<BaseTreeViewItem> items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            var favorites = FavoriteManager.GetFavorites();

            var inserts = new List<Favorite>();
            foreach (var item in items)
            {
                var favoriteItem = (FavoriteTreeViewItem)item;

                inserts.Add(favoriteItem.Favorite);

                favorites.Remove(favoriteItem.Favorite);
            }

            inserts.Reverse();

            var index = favorites.IndexOf(Favorite);
            if (adorner.LastPosition == ControlDragAdornerPosition.Bottom)
            {
                index++;
            }

            if (index < 0)
            {
                index = 0;
            }

            foreach (var favorite in inserts)
            {
                favorites.Insert(index, favorite);
            }

            FavoriteManager.Save();

            var parent = (ItemsControl)Parent;
            for (var i = 0; i < items.Count; i++)
            {
                var item = (FavoriteTreeViewItem)items[i];

                parent.Items.Remove(item);
                parent.Items.Insert(index, item);
            }
        }

        private void DropItems([NotNull] IEnumerable<IItem> items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            var favorites = FavoriteManager.GetFavorites();

            var index = favorites.IndexOf(Favorite);
            if (adorner.LastPosition == ControlDragAdornerPosition.Bottom)
            {
                index++;
            }

            ((FavoriteRootTreeViewItem)Parent).AddFavorites(items, index);
        }

        private void Locate([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
            {
                return;
            }

            var treeView = this.GetAncestorOrSelf<ItemTreeView>();
            if (treeView != null)
            {
                treeView.ExpandTo(Favorite.ItemVersionUri.ItemUri);
            }
        }

        private void Open([NotNull] object sender, [NotNull] MouseButtonEventArgs mouseButtonEventArgs)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(mouseButtonEventArgs, nameof(mouseButtonEventArgs));

            AppHost.OpenContentEditor(Favorite.ItemVersionUri);
        }

        private void OpenToolTip([NotNull] object sender, [NotNull] ToolTipEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ToolTip = ToolTipBuilder.BuildToolTip(this);
        }
    }
}
