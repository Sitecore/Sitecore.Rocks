// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentEditors.Pipelines.LoadItems
{
    [Pipeline(typeof(LoadItemsPipeline), 2000)]
    public class GetContentModel : PipelineProcessor<LoadItemsPipeline>
    {
        protected override void Process(LoadItemsPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            pipeline.ContentModel = new ContentModel
            {
                UriList = pipeline.Items
            };

            if (pipeline.Items.Count == 0)
            {
                return;
            }

            var firstItemUri = pipeline.Items[0];
            pipeline.Site = firstItemUri.ItemUri.Site;
        }
    }
}
