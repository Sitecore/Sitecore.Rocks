// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.ContentTrees.VirtualItems;

namespace Sitecore.Rocks.UI.Libraries.ContentTrees.VirtualItems
{
    [VirtualItem(3000)]
    public class LibrariesVirtualItem : IVirtualItem
    {
        public bool CanAddItem(BaseTreeViewItem parent)
        {
            return parent == null;
        }

        public BaseTreeViewItem GetItem(BaseTreeViewItem parent)
        {
            var result = new LibrariesRootTreeViewItem();

            result.Items.Add(DummyTreeViewItem.Instance);

            return result;
        }
    }
}
