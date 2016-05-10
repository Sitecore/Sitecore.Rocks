// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;

namespace Sitecore.Rocks.ContentTrees.VirtualItems.RecentItems
{
    [VirtualItem(500)]
    public class HistoryVirtualItem : IVirtualItem
    {
        public bool CanAddItem(BaseTreeViewItem parent)
        {
            return parent is RecentItemsTreeViewItem;
        }

        public BaseTreeViewItem GetItem([CanBeNull] BaseTreeViewItem parent)
        {
            var result = new HistoryTreeViewItem();

            result.Items.Add(DummyTreeViewItem.Instance);

            return result;
        }
    }
}
