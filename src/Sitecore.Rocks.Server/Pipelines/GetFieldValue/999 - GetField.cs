// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility.Pipelines;

namespace Sitecore.Rocks.Server.Pipelines.GetFieldValue
{
    [Pipeline(typeof(GetFieldValuePipeline), 9999)]
    public class GetField : PipelineProcessor<GetFieldValuePipeline>
    {
        protected override void Process(GetFieldValuePipeline pipeline)
        {
            Assert.ArgumentNotNull(pipeline, nameof(pipeline));

            pipeline.Value = pipeline.Field.Value;
        }
    }
}
