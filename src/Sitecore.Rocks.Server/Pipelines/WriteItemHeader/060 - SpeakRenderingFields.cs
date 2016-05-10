// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility.Pipelines;
using Sitecore.Rocks.Server.Layouts;

namespace Sitecore.Rocks.Server.Pipelines.WriteItemHeader
{
    [Pipeline(typeof(WriteItemHeaderPipeline), 6000)]
    public class SpeakRenderingFields : PipelineProcessor<WriteItemHeaderPipeline>
    {
        public static readonly ID SpeakRenderingFolderId = new ID(new Guid("{BAAD9D10-19E7-4878-A96F-E290B914BF5F}"));

        public static readonly ID ViewRenderingId = new ID(new Guid("{99F8905D-4A87-4EB8-9F8B-A9BEBFB3ADD6}"));

        protected override void Process(WriteItemHeaderPipeline pipeline)
        {
            Assert.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.Item.TemplateID != ViewRenderingId)
            {
                return;
            }

            var helper = new SpeakCoreVersionHelper();

            var versionItem = helper.GetSpeakCoreVersionItem(pipeline.Item.Parent);
            if (versionItem == null)
            {
                return;
            }

            pipeline.Output.WriteAttributeString("ex.speakversion", versionItem.Name);
            pipeline.Output.WriteAttributeString("ex.speakversionid", versionItem.ID.ToString());
        }
    }
}
