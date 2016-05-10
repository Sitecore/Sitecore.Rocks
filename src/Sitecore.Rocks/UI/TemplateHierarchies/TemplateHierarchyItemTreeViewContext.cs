// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.TemplateHierarchies
{
    public class TemplateHierarchyItemTreeViewContext : ContentTreeContext
    {
        public TemplateHierarchyItemTreeViewContext([NotNull] ItemTreeView contentTree, [NotNull] IEnumerable<BaseTreeViewItem> selectedItems, [NotNull] TemplateHierarchyTab templateHierarchyTab) : base(contentTree, selectedItems)
        {
            Assert.ArgumentNotNull(contentTree, nameof(contentTree));
            Assert.ArgumentNotNull(selectedItems, nameof(selectedItems));
            Assert.ArgumentNotNull(templateHierarchyTab, nameof(templateHierarchyTab));

            TemplateHierarchyTab = templateHierarchyTab;
        }

        public TemplateHierarchyTab TemplateHierarchyTab { get; set; }
    }
}
