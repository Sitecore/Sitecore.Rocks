// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Text;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility.Pipelines;

namespace Sitecore.Rocks.Server.Pipelines.WriteItemHeader
{
    [Pipeline(typeof(WriteItemHeaderPipeline), 2000)]
    public class WebControlFields : PipelineProcessor<WriteItemHeaderPipeline>
    {
        private static readonly ID webControlId = new ID("{1DDE3F02-0BD7-4779-867A-DC578ADF91EA}");

        protected override void Process(WriteItemHeaderPipeline pipeline)
        {
            Assert.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.Item.TemplateID != webControlId)
            {
                return;
            }

            var prefix = pipeline.Item["TagPrefix"];
            var tag = pipeline.Item["Tag"];

            var sb = new StringBuilder();
            sb.Append('<');

            if (!string.IsNullOrEmpty(prefix))
            {
                sb.Append(prefix);
                sb.Append(":");
            }

            sb.Append(tag);

            sb.Append(" runat=\"server\" />");

            pipeline.Output.WriteAttributeString("ex.dragdrop.text", sb.ToString());
        }
    }
}
