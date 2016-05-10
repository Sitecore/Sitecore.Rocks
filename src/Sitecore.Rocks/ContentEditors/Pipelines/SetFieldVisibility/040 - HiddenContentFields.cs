// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentEditors.Pipelines.SetFieldVisibility
{
    [Pipeline(typeof(SetFieldVisibilityPipeline), 4000)]
    public class HiddenContentFields : PipelineProcessor<SetFieldVisibilityPipeline>
    {
        protected override void Process(SetFieldVisibilityPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (!pipeline.Appearance.StandardFields && !FieldManager.IsStandardField(pipeline.Field) && !FieldManager.GetContentFieldVisibility(pipeline.Field.TemplateFieldId))
            {
                pipeline.IsVisible = false;
            }
        }
    }
}
