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
    [Pipeline(typeof(ProcessCodeFirstTemplatesPipeline), 1000)]
    public class ProcessClasses : PipelineProcessor<ProcessCodeFirstTemplatesPipeline>
    {
        protected virtual void GetBaseClasses([NotNull] FileCodeModel fileCodeModel, [NotNull] CodeClass2 codeClass, [NotNull] ICollection<string> baseTemplates)
        {
            Debug.ArgumentNotNull(fileCodeModel, nameof(fileCodeModel));
            Debug.ArgumentNotNull(codeClass, nameof(codeClass));
            Debug.ArgumentNotNull(baseTemplates, nameof(baseTemplates));

            foreach (var baseClass in codeClass.Bases.OfType<CodeClass2>())
            {
                if (baseClass.Name == "Object")
                {
                    continue;
                }

                var baseTemplateId = fileCodeModel.GetHash(baseClass.Attributes, baseClass.Name).Format();
                baseTemplates.Add(baseTemplateId);
            }
        }

        protected virtual void GetImplementedInterface([NotNull] CodeInterface2 implementedInterface, [NotNull] ICollection<string> baseFields)
        {
            Debug.ArgumentNotNull(implementedInterface, nameof(implementedInterface));
            Debug.ArgumentNotNull(baseFields, nameof(baseFields));

            foreach (var property in implementedInterface.Members.OfType<CodeProperty2>())
            {
                if (!baseFields.Contains(property.Name))
                {
                    baseFields.Add(property.Name);
                }
            }

            foreach (var i in implementedInterface.Bases.OfType<CodeInterface2>())
            {
                GetImplementedInterface(i, baseFields);
            }
        }

        protected virtual void GetImplementedInterfaces([NotNull] FileCodeModel fileCodeModel, [NotNull] CodeClass2 codeClass, [NotNull] ICollection<string> baseTemplates, [NotNull] ICollection<string> baseFields)
        {
            Debug.ArgumentNotNull(fileCodeModel, nameof(fileCodeModel));
            Debug.ArgumentNotNull(codeClass, nameof(codeClass));
            Debug.ArgumentNotNull(baseTemplates, nameof(baseTemplates));
            Debug.ArgumentNotNull(baseFields, nameof(baseFields));

            foreach (var implementedInterface in codeClass.ImplementedInterfaces.OfType<CodeInterface2>())
            {
                var baseTemplateId = fileCodeModel.GetHash(implementedInterface.Attributes, implementedInterface.Name).Format();
                baseTemplates.Add(baseTemplateId);

                GetImplementedInterface(implementedInterface, baseFields);
            }
        }

        protected override void Process(ProcessCodeFirstTemplatesPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            foreach (var codeClass in pipeline.FileCodeModel.CodeElements.OfType<CodeClass2>())
            {
                var template = new Template(codeClass);

                ProcessTemplate(pipeline.FileCodeModel, template, codeClass);

                pipeline.Templates.Add(template);
            }
        }

        protected virtual void ProcessClassAttributes([NotNull] CodeClass2 codeClass, [NotNull] Template template)
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

        protected virtual void ProcessTemplate([NotNull] FileCodeModel fileCodeModel, [NotNull] Template template, [NotNull] CodeClass2 codeClass)
        {
            Debug.ArgumentNotNull(fileCodeModel, nameof(fileCodeModel));
            Debug.ArgumentNotNull(template, nameof(template));
            Debug.ArgumentNotNull(codeClass, nameof(codeClass));

            template.Name = codeClass.Name;
            template.TemplateItemId = new ItemId(fileCodeModel.GetHash(codeClass.Attributes, codeClass.Name));

            var baseTemplates = new List<string>();
            var baseFields = new List<string>();

            GetBaseClasses(fileCodeModel, codeClass, baseTemplates);
            GetImplementedInterfaces(fileCodeModel, codeClass, baseTemplates, baseFields);
            ProcessClassAttributes(codeClass, template);

            template.BaseTemplates = string.Join("|", baseTemplates);

            var isGlassMapper = codeClass.Attributes.OfType<CodeAttribute2>().Any(a => a.Name == "SitecoreClass" || a.Name == "SitecoreClassAttribute");

            ProcessTemplateSections(fileCodeModel, template, baseFields, codeClass, isGlassMapper);
        }

        protected virtual void ProcessTemplateField([NotNull] FileCodeModel fileCodeModel, [NotNull] Template template, [NotNull] TemplateSection templateSection, [NotNull] CodeProperty2 property, bool isGlassMapper)
        {
            Debug.ArgumentNotNull(fileCodeModel, nameof(fileCodeModel));
            Debug.ArgumentNotNull(template, nameof(template));
            Debug.ArgumentNotNull(templateSection, nameof(templateSection));
            Debug.ArgumentNotNull(property, nameof(property));

            if ((property.Access & vsCMAccess.vsCMAccessPublic) != vsCMAccess.vsCMAccessPublic)
            {
                return;
            }

            var type = property.Type.AsFullName;
            var handler = FieldTypeHandlerManager.FieldTypes.FirstOrDefault(h => h.CanHandle(type));
            if (handler == null)
            {
                return;
            }

            if (isGlassMapper)
            {
                // ignore Glass Mapper info fields
                if (property.Attributes.OfType<CodeAttribute2>().Any(a => a.Name == "SitecoreInfo" || a.Name == "SitecoreInfoAttribute"))
                {
                    return;
                }

                // ignore properties without SitecoreField attribute
                if (!property.Attributes.OfType<CodeAttribute2>().Any(a => a.Name == "SitecoreField" || a.Name == "SitecoreFieldAttribute"))
                {
                    return;
                }
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

        protected virtual void ProcessTemplateSections([NotNull] FileCodeModel fileCodeModel, [NotNull] Template template, [NotNull] IEnumerable<string> baseFields, [NotNull] CodeClass2 codeClass, bool isGlassMapper)
        {
            Debug.ArgumentNotNull(fileCodeModel, nameof(fileCodeModel));
            Debug.ArgumentNotNull(template, nameof(template));
            Debug.ArgumentNotNull(baseFields, nameof(baseFields));
            Debug.ArgumentNotNull(codeClass, nameof(codeClass));

            var templateSection = new TemplateSection();
            template.Sections.Add(templateSection);

            templateSection.Name = "Data";
            templateSection.TemplateSectionItemId = new ItemId(GuidExtensions.Hash(template.Name + @"/" + templateSection.Name));

            foreach (var property in codeClass.Members.OfType<CodeProperty2>())
            {
                if (!baseFields.Contains(property.Name))
                {
                    ProcessTemplateField(fileCodeModel, template, templateSection, property, isGlassMapper);
                }
            }
        }
    }
}
