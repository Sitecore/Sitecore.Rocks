// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentTrees.Pipelines.DeleteItem
{
    [Pipeline(typeof(DeleteItemPipeline), 1000)]
    public class DeleteItem : PipelineProcessor<DeleteItemPipeline>
    {
        protected override void Process(DeleteItemPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            pipeline.IsDeleted = pipeline.ItemUri.Site.DataService.Delete(pipeline.ItemUri);
        }
    }
}
