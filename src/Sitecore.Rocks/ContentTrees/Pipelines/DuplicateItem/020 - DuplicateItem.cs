// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentTrees.Pipelines.DuplicateItem
{
    [Pipeline(typeof(DuplicateItemPipeline), 2000)]
    public class DuplicateItem : PipelineProcessor<DuplicateItemPipeline>
    {
        protected override void Process([NotNull] DuplicateItemPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            pipeline.NewItemUri = pipeline.ItemUri.Site.DataService.Duplicate(pipeline.ItemUri, pipeline.NewName);

            Notifications.RaiseItemDuplicated(this, pipeline.NewItemUri, pipeline.ItemUri);
        }
    }
}
