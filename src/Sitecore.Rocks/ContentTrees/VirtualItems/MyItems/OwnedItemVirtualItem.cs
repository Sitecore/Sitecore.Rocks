// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.VirtualItems.MyItems
{
    [VirtualItem(4000)]
    public class OwnedItemVirtualItem : IVirtualItem
    {
        public bool CanAddItem(BaseTreeViewItem parent)
        {
            return parent is MyItemsTreeViewItem;
        }

        public BaseTreeViewItem GetItem([CanBeNull] BaseTreeViewItem parent)
        {
            var databaseItem = parent as MyItemsTreeViewItem;
            if (databaseItem == null)
            {
                throw Exceptions.InvalidOperation();
            }

            var result = new OwnedItemTreeViewItem
            {
                DatabaseUri = databaseItem.DatabaseUri
            };

            result.Items.Add(DummyTreeViewItem.Instance);

            return result;
        }
    }
}
