// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.ContentTrees.VirtualItems.RecentItems;

namespace Sitecore.Rocks.ContentTrees.VirtualItems.Searches
{
    [VirtualItem(3000)]
    public class SearchesVirtualItem : IVirtualItem
    {
        public bool CanAddItem(BaseTreeViewItem parent)
        {
            return parent is RecentItemsTreeViewItem;
        }

        public BaseTreeViewItem GetItem([CanBeNull] BaseTreeViewItem parent)
        {
            var result = new SearchRootTreeViewItem();

            result.Items.Add(DummyTreeViewItem.Instance);

            return result;
        }
    }
}
