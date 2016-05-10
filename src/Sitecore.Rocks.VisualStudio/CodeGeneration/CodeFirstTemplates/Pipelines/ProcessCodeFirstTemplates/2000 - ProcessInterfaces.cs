// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.CodeGeneration.CodeFirstTemplates.FieldTypes;
using Sitecore.Rocks.CodeGeneration.CodeFirstTemplates.Templates;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Extensions.GuidExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.CodeGeneration.CodeFirstTemplates.Pipelines.ProcessCodeFirstTemplates
{
    [Pipeline(typeof(ProcessCodeFirstTemplatesPipeline), 2000)]
    public class ProcessInterfaces : PipelineProcessor<ProcessCodeFirstTemplatesPipeline>
    {
        protected virtual void GetImplementedInterfaces([NotNull] FileCodeModel fileCodeModel, [NotNull] CodeInterface2 codeInterface, [NotNull] ICollection<string> baseTemplates)
        {
            Debug.ArgumentNotNull(fileCodeModel, nameof(fileCodeModel));
            Debug.ArgumentNotNull(codeInterface, nameof(codeInterface));
            Debug.ArgumentNotNull(baseTemplates, nameof(baseTemplates));

            foreach (var implementedInterface in codeInterface.Bases.OfType<CodeInterface2>())
            {
                var baseTemplateId = fileCodeModel.GetHash(implementedInterface.Attributes, implementedInterface.Name).Format();
                baseTemplates.Add(baseTemplateId);
            }
        }

        protected override void Process(ProcessCodeFirstTemplatesPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            foreach (var codeInterface in pipeline.FileCodeModel.CodeElements.OfType<CodeInterface2>())
            {
                var template = new Template(codeInterface);

                ProcessTemplate(pipeline.FileCodeModel, template, codeInterface);

                pipeline.Templates.Add(template);
            }
        }

        protected virtual void ProcessClassAttributes([NotNull] CodeInterface2 codeClass, [NotNull] Template template)
        {
            Debug.ArgumentNotNull(codeClass, nameof(codeClass));
            Debug.ArgumentNotNull(template, nameof(template));

            var sitecoreClass = codeClass.Attributes.OfType<CodeAttribute2>().FirstOrDefault(a => a.Name == "SitecoreClass" || a.Name == "SitecoreClassAttribute");
            if (sitecoreClass != null)
            {
                foreach (var argument in sitecoreClass.Arguments.OfType<CodeAttributeArgument>())
                {
                    var value = argument.Value;

                    if (value.StartsWith("\"") && value.EndsWith("\""))
                    {
                        value = value.Mid(1, value.Length - 2);
                    }

                    if (argument.Name == "ParentPath")
                    {
                        template.ParentPath = value;
                    }
                }
            }

            var parentPath = codeClass.Attributes.OfType<CodeAttribute2>().FirstOrDefault(a => a.Name == "ParentPath" || a.Name == "ParentPathAttribute");
            if (parentPath != null)
            {
                var argument = parentPath.Arguments.OfType<CodeAttribute>().FirstOrDefault();
                if (argument != null)
                {
                    var value = argument.Value;

                    if (value.StartsWith("\"") && value.EndsWith("\""))
                    {
                        value = value.Mid(1, value.Length - 2);
                    }

                    template.ParentPath = value;
                }
            }
        }

        protected virtual void ProcessTemplate([NotNull] FileCodeModel fileCodeModel, [NotNull] Template template, [NotNull] CodeInterface2 codeInterface)
        {
            Debug.ArgumentNotNull(fileCodeModel, nameof(fileCodeModel));
            Debug.ArgumentNotNull(template, nameof(template));
            Debug.ArgumentNotNull(codeInterface, nameof(codeInterface));

            template.Name = codeInterface.Name;
            template.TemplateItemId = new ItemId(fileCodeModel.GetHash(codeInterface.Attributes, codeInterface.Name));

            var baseTemplates = new List<string>();
            GetImplementedInterfaces(fileCodeModel, codeInterface, baseTemplates);
            ProcessClassAttributes(codeInterface, template);

            template.BaseTemplates = string.Join("|", baseTemplates);

            ProcessTemplateSections(fileCodeModel, template, codeInterface);
        }

        protected virtual void ProcessTemplateField([NotNull] FileCodeModel fileCodeModel, [NotNull] Template template, [NotNull] TemplateSection templateSection, [NotNull] CodeProperty2 property)
        {
            Debug.ArgumentNotNull(fileCodeModel, nameof(fileCodeModel));
            Debug.ArgumentNotNull(template, nameof(template));
            Debug.ArgumentNotNull(templateSection, nameof(templateSection));
            Debug.ArgumentNotNull(property, nameof(property));

            var type = property.Type.AsFullName;
            var handler = FieldTypeHandlerManager.FieldTypes.FirstOrDefault(h => h.CanHandle(type));
            if (handler == null)
            {
                return;
            }

            var field = new TemplateField(property);
            templateSection.Fields.Add(field);

            field.Name = property.Name;
            field.TemplateFieldItemId = new ItemId(fileCodeModel.GetHash(property.Attributes, template.Name + @"/" + templateSection.Name + @"/" + field.Name));
            field.Type = string.Empty;
            field.Source = string.Empty;
            field.Shared = false;
            field.Unversioned = false;
            field.Title = string.Empty;
            field.ValidatorBar = string.Empty;

            handler.Handle(type, field);

            var sitecoreField = property.Attributes.OfType<CodeAttribute2>().FirstOrDefault(a => a.Name == "SitecoreField" || a.Name == "SitecoreFieldAttribute");
            if (sitecoreField == null)
            {
                return;
            }

            var index = 0;
            foreach (var argument in sitecoreField.Arguments.OfType<CodeAttributeArgument>())
            {
                var value = argument.Value;

                if (value.StartsWith("\"") && value.EndsWith("\""))
                {
                    value = value.Mid(1, value.Length - 2);
                }

                if (string.IsNullOrEmpty(argument.Name) && index == 0)
                {
                    field.Name = value;
                }

                if (argument.Name == "FieldName")
                {
                    field.Name = value;
                }

                if (argument.Name == "Type")
                {
                    field.Type = value;
                }

                if (argument.Name == "Shared")
                {
                    field.Shared = value == "true";
                }

                if (argument.Name == "Unversioned")
                {
                    field.Shared = value == "true";
                }

                if (argument.Name == "Source")
                {
                    field.Source = value;
                }

                index++;
            }
        }

        protected virtual void ProcessTemplateSections([NotNull] FileCodeModel fileCodeModel, [NotNull] Template template, [NotNull] CodeInterface2 codeInterface)
        {
            Debug.ArgumentNotNull(fileCodeModel, nameof(fileCodeModel));
            Debug.ArgumentNotNull(template, nameof(template));
            Debug.ArgumentNotNull(codeInterface, nameof(codeInterface));

            var templateSection = new TemplateSection();
            template.Sections.Add(templateSection);

            templateSection.Name = "Data";
            templateSection.TemplateSectionItemId = new ItemId(GuidExtensions.Hash(template.Name + @"/" + templateSection.Name));

            foreach (var property in codeInterface.Members.OfType<CodeProperty2>())
            {
                ProcessTemplateField(fileCodeModel, template, templateSection, property);
            }
        }
    }
}
