// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility.Pipelines;

namespace Sitecore.Rocks.Server.Pipelines.WriteItemHeader
{
    [Pipeline(typeof(WriteItemHeaderPipeline), 2000)]
    public class XslRenderingFields : PipelineProcessor<WriteItemHeaderPipeline>
    {
        protected override void Process([NotNull] WriteItemHeaderPipeline pipeline)
        {
            Assert.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.Item.TemplateID == TemplateIDs.XSLRendering)
            {
                pipeline.Output.WriteAttributeString("ex.dragdrop.text", "<sc:xslfile runat=\"server\" path=\"" + pipeline.Item["Path"] + "\" />");
            }
        }
    }
}
