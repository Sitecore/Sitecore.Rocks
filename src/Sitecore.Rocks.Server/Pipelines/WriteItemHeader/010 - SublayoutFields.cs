// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility.Pipelines;

namespace Sitecore.Rocks.Server.Pipelines.WriteItemHeader
{
    [Pipeline(typeof(WriteItemHeaderPipeline), 1000)]
    public class SublayoutFields : PipelineProcessor<WriteItemHeaderPipeline>
    {
        protected override void Process(WriteItemHeaderPipeline pipeline)
        {
            Assert.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.Item.TemplateID == TemplateIDs.Sublayout)
            {
                pipeline.Output.WriteAttributeString("ex.dragdrop.text", "<sc:sublayout runat=\"server\" path=\"" + pipeline.Item["Path"] + "\" />");
            }
        }
    }
}
