// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Managers;
using Sitecore.Data.Templates;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility.Pipelines;

namespace Sitecore.Rocks.Server.Pipelines.WriteItemHeader
{
    [Pipeline(typeof(WriteItemHeaderPipeline), 4000)]
    public class AltTextFields : PipelineProcessor<WriteItemHeaderPipeline>
    {
        protected override void Process(WriteItemHeaderPipeline pipeline)
        {
            var template = TemplateManager.GetTemplate(pipeline.Item.TemplateID, pipeline.Item.Database);

            try
            {
                WriteAltText(pipeline, template);
            }
            catch
            {
                return;
            }
        }

        private void WriteAltText([NotNull] WriteItemHeaderPipeline pipeline, [NotNull] Template template)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));
            Debug.ArgumentNotNull(template, nameof(template));

            if (template.InheritsFrom(TemplateIDs.UnversionedImage) || template.InheritsFrom(TemplateIDs.VersionedImage))
            {
                pipeline.Output.WriteAttributeString("ex.alt.text", pipeline.Item["Alt"]);
            }
        }
    }
}
