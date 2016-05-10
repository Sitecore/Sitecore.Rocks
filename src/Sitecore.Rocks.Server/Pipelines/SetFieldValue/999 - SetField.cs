// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility.Pipelines;

namespace Sitecore.Rocks.Server.Pipelines.SetFieldValue
{
    [Pipeline(typeof(SetFieldValuePipeline), 9999)]
    public class SetField : PipelineProcessor<SetFieldValuePipeline>
    {
        protected override void Process(SetFieldValuePipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            pipeline.Field.Value = pipeline.Value;
        }
    }
}
