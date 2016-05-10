// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Gutters
{
    public class GutterContext : ICommandContext
    {
        public GutterContext([NotNull] ItemTreeView contentTree, [NotNull] IEnumerable<BaseTreeViewItem> selectedItems)
        {
            Assert.ArgumentNotNull(contentTree, nameof(contentTree));
            Assert.ArgumentNotNull(selectedItems, nameof(selectedItems));

            SelectedItems = selectedItems;
            ContentTree = contentTree;
        }

        [NotNull]
        public ItemTreeView ContentTree { get; private set; }

        [NotNull]
        public IEnumerable<BaseTreeViewItem> SelectedItems { get; private set; }
    }
}
