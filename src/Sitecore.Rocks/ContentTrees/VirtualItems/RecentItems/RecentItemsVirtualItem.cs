// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;

namespace Sitecore.Rocks.ContentTrees.VirtualItems.RecentItems
{
    [VirtualItem(2000)]
    public class RecentItemsVirtualItem : IVirtualItem
    {
        public bool CanAddItem(BaseTreeViewItem parent)
        {
            return false;
        }

        public BaseTreeViewItem GetItem([CanBeNull] BaseTreeViewItem parent)
        {
            var result = new RecentItemsTreeViewItem();

            result.Items.Add(DummyTreeViewItem.Instance);

            return result;
        }
    }
}
