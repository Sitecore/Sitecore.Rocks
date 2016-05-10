// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Data.Fields;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility.Pipelines;

namespace Sitecore.Rocks.Server.Pipelines.GetFieldValue
{
    [Pipeline(typeof(GetFieldValuePipeline), 1000)]
    public class SetLayoutField : PipelineProcessor<GetFieldValuePipeline>
    {
        protected override void Process(GetFieldValuePipeline pipeline)
        {
            Assert.ArgumentNotNull(pipeline, nameof(pipeline));

            if (string.Compare(pipeline.Field.Type, "Layout", StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                return;
            }

            try
            {
                pipeline.Value = GetField(pipeline);
            }
            catch (MissingMethodException)
            {
                return;
            }

            pipeline.Abort();
        }

        [NotNull]
        private string GetField([NotNull] GetFieldValuePipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            return LayoutField.GetFieldValue(pipeline.Field);
        }
    }
}
