// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentTrees.Pipelines.RenameItem
{
    public class RenameItemPipeline : Pipeline<RenameItemPipeline>
    {
        public bool IsRenamed { get; set; }

        [NotNull]
        public ItemUri ItemUri { get; set; }

        [NotNull]
        public string NewName { get; set; }
    }
}
