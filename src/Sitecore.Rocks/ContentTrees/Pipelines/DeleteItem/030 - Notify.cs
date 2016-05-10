// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentTrees.Pipelines.DeleteItem
{
    [Pipeline(typeof(DeleteItemPipeline), 3000)]
    public class Notify : PipelineProcessor<DeleteItemPipeline>
    {
        protected override void Process(DeleteItemPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (!pipeline.IsDeleted)
            {
                return;
            }

            Notifications.RaiseItemDeleted(this, pipeline.ItemUri);
        }
    }
}
