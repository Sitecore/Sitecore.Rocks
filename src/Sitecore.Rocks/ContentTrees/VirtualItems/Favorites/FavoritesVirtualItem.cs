// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.ContentTrees.Items;

namespace Sitecore.Rocks.ContentTrees.VirtualItems.Favorites
{
    [VirtualItem(1000)]
    public class FavoritesVirtualItem : IVirtualItem
    {
        public bool CanAddItem(BaseTreeViewItem parent)
        {
            return parent == null;
        }

        public BaseTreeViewItem GetItem(BaseTreeViewItem parent)
        {
            var result = new FavoriteRootTreeViewItem();

            result.Items.Add(DummyTreeViewItem.Instance);

            return result;
        }
    }
}
