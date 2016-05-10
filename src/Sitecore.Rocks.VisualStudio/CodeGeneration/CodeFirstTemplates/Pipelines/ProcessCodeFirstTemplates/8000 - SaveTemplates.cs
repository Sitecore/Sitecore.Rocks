// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.CodeGeneration.CodeFirstTemplates.Templates;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Extensions.DataServiceExtensions;

namespace Sitecore.Rocks.CodeGeneration.CodeFirstTemplates.Pipelines.ProcessCodeFirstTemplates
{
    [Pipeline(typeof(ProcessCodeFirstTemplatesPipeline), 8000)]
    public class SaveTemplates : PipelineProcessor<ProcessCodeFirstTemplatesPipeline>
    {
        protected override void Process(ProcessCodeFirstTemplatesPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            var databaseUri = new DatabaseUri(pipeline.Site, DatabaseName.Master);

            foreach (var template in pipeline.Templates)
            {
                Save(databaseUri, template);
            }
        }

        private void Save([NotNull] DatabaseUri databaseUri, [NotNull] Template template)
        {
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Debug.ArgumentNotNull(template, nameof(template));

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);
            output.Formatting = Formatting.Indented;

            var templateName = template.Name;
            var templateId = template.TemplateItemId.ToString();

            output.WriteStartElement(@"template");

            output.WriteAttributeString(@"name", templateName);
            output.WriteAttributeString(@"id", templateId);
            output.WriteAttributeString(@"icon", template.Icon);
            output.WriteAttributeString(@"basetemplates", template.BaseTemplates);
            output.WriteAttributeString(@"parentpath", template.ParentPath);

            foreach (var templateSection in template.Sections)
            {
                var templateSectionName = templateSection.Name;
                var templateSectionId = templateSection.TemplateSectionItemId.ToString();

                output.WriteStartElement(@"section");

                output.WriteAttributeString(@"name", templateSectionName);
                output.WriteAttributeString(@"id", templateSectionId);

                foreach (var field in templateSection.Fields)
                {
                    var fieldName = field.Name;
                    var fieldId = field.TemplateFieldItemId.ToString();

                    output.WriteStartElement(@"field");

                    output.WriteAttributeString(@"id", fieldId);
                    output.WriteAttributeString(@"name", fieldName);
                    output.WriteAttributeString(@"type", field.Type);
                    output.WriteAttributeString(@"source", field.Source);
                    output.WriteAttributeString(@"shared", field.Shared ? "1" : "0");
                    output.WriteAttributeString(@"unversioned", field.Unversioned ? @"1" : "0");
                    output.WriteAttributeString(@"title", field.Title);
                    output.WriteAttributeString(@"validatorbar", field.ValidatorBar);

                    output.WriteEndElement();
                }

                output.WriteEndElement();
            }

            output.WriteEndElement();

            GetValueCompleted<string> completed = delegate { };

            databaseUri.Site.DataService.SaveTemplateXml(writer.ToString(), databaseUri, completed);
        }
    }
}
