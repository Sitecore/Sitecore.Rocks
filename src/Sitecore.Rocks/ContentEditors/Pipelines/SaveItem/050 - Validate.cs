// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentEditors.Pipelines.SaveItem
{
    [Pipeline(typeof(SaveItemPipeline), 5000)]
    public class Validate : PipelineProcessor<SaveItemPipeline>
    {
        protected override void Process(SaveItemPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            pipeline.Editor.Validate(pipeline.ContentModel);
        }
    }
}
