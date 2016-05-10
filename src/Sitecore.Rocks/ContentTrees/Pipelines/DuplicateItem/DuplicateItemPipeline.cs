// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentTrees.Pipelines.DuplicateItem
{
    public class DuplicateItemPipeline : Pipeline<DuplicateItemPipeline>
    {
        public ItemUri ItemUri { get; set; }

        public ItemUri NewItemUri { get; set; }

        public string NewName { get; set; }

        public ItemTreeViewItem TreeViewItem { get; set; }
    }
}
