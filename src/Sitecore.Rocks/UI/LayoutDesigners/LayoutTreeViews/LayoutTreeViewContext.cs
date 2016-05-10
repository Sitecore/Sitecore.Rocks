// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews
{
    public class LayoutTreeViewContext : LayoutDesignerContext
    {
        public LayoutTreeViewContext([NotNull] LayoutDesigner layoutDesigner, [NotNull] IEnumerable<BaseTreeViewItem> treeViewItems, [CanBeNull] LayoutDesignerItem selectedItem, [NotNull] IEnumerable<LayoutDesignerItem> selectedItems) : base(layoutDesigner, selectedItem, selectedItems)
        {
            Assert.ArgumentNotNull(layoutDesigner, nameof(layoutDesigner));
            Assert.ArgumentNotNull(treeViewItems, nameof(treeViewItems));
            Assert.ArgumentNotNull(selectedItems, nameof(selectedItems));

            TreeViewItems = treeViewItems;
        }

        [NotNull]
        public IEnumerable<BaseTreeViewItem> TreeViewItems { get; private set; }
    }
}
