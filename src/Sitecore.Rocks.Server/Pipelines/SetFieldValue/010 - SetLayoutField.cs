// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Data.Fields;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility.Pipelines;

namespace Sitecore.Rocks.Server.Pipelines.SetFieldValue
{
    [Pipeline(typeof(SetFieldValuePipeline), 1000)]
    public class SetLayoutField : PipelineProcessor<SetFieldValuePipeline>
    {
        protected override void Process(SetFieldValuePipeline pipeline)
        {
            Assert.ArgumentNotNull(pipeline, nameof(pipeline));

            if (string.Compare(pipeline.Field.Type, "Layout", StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                return;
            }

            if (string.IsNullOrEmpty(pipeline.Value))
            {
                return;
            }

            try
            {
                SetField(pipeline);
            }
            catch (MissingMethodException)
            {
                return;
            }

            pipeline.Abort();
        }

        private void SetField([NotNull] SetFieldValuePipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            LayoutField.SetFieldValue(pipeline.Field, pipeline.Value);
        }
    }
}
