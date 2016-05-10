// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentTrees.Pipelines.DeleteItem
{
    public class DeleteItemPipeline : Pipeline<DeleteItemPipeline>
    {
        public bool DeleteFiles { get; set; }

        public bool IsDeleted { get; set; }

        [NotNull]
        public ItemUri ItemUri { get; set; }
    }
}
