// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentTrees.Pipelines.RenameItem
{
    [Pipeline(typeof(RenameItemPipeline), 2000)]
    public class Notify : PipelineProcessor<RenameItemPipeline>
    {
        protected override void Process(RenameItemPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.IsRenamed)
            {
                Notifications.RaiseItemRenamed(this, pipeline.ItemUri, pipeline.NewName);
            }
        }
    }
}
