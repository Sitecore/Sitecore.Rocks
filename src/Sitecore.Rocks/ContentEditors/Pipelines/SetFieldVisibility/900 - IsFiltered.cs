// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentEditors.Pipelines.SetFieldVisibility
{
    [Pipeline(typeof(SetFieldVisibilityPipeline), 9000)]
    public class IsFiltered : PipelineProcessor<SetFieldVisibilityPipeline>
    {
        protected override void Process([NotNull] SetFieldVisibilityPipeline pipeline)
        {
            Assert.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.Field.IsFiltered)
            {
                pipeline.IsVisible = false;
            }
        }
    }
}
