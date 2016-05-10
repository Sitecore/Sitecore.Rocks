// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentTrees.VirtualItems.MyItems
{
    [VirtualItem(1000), Feature(FeatureNames.SitecoreExplorer.MyItems)]
    public class MyItemsVirtualItem : IVirtualItem
    {
        public bool CanAddItem(BaseTreeViewItem parent)
        {
            var item = parent as ItemTreeViewItem;
            if (item == null)
            {
                return false;
            }

            return item.ItemUri.ItemId.ToGuid() == DatabaseTreeViewItem.RootItemGuid;
        }

        public BaseTreeViewItem GetItem(BaseTreeViewItem parent)
        {
            var databaseItem = parent as ItemTreeViewItem;
            if (databaseItem == null)
            {
                throw Exceptions.InvalidOperation();
            }

            var result = new MyItemsTreeViewItem
            {
                DatabaseUri = databaseItem.ItemUri.DatabaseUri
            };

            result.Items.Add(DummyTreeViewItem.Instance);

            return result;
        }
    }
}
