// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.CodeGeneration.TemplateClasses.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers), DisplayName(@"Template"), DefaultProperty("ClassName"), Description("Template")]
    public class Template
    {
        public Template([NotNull] IShapeCreator shapeCreator, [NotNull] ItemUri templateUri)
        {
            Assert.ArgumentNotNull(shapeCreator, nameof(shapeCreator));
            Assert.ArgumentNotNull(templateUri, nameof(templateUri));

            ShapeCreator = shapeCreator;
            TemplateUri = templateUri;
            TemplateSections = new List<TemplateSection>();
        }

        [Category("Code Generation"), DisplayName("Class Name"), Description("The name of the generated class.")]
        public string ClassName { get; set; }

        [Category("Template"), ReadOnly(true), Description("The name of the template."), DisplayName("Template Name")]
        public string Name { get; set; }

        [Browsable(false)]
        public List<TemplateSection> TemplateSections { get; private set; }

        [NotNull, Browsable(false)]
        public ItemUri TemplateUri { get; private set; }

        protected IShapeCreator ShapeCreator { get; set; }

        public virtual void Load([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            TemplateSections.Clear();

            ItemUri templateUri;
            ItemUri.TryParse(element.GetAttributeValue("templateuri"), out templateUri);

            TemplateUri = templateUri;
            Name = element.GetAttributeValue("templatename");
            ClassName = element.GetAttributeValue("classname");

            foreach (var sectionElement in element.Elements())
            {
                var section = ShapeCreator.CreateTemplateSection(ShapeCreator);

                section.Load(sectionElement);

                TemplateSections.Add(section);
            }
        }

        public virtual void Parse([NotNull] XElement element)
        {
            Debug.ArgumentNotNull(element, nameof(element));

            // TODO: store properties in derrived classes - like MvcModel
            var className = ClassName;
            var propertyNames = new Dictionary<string, string>();
            foreach (var templateField in TemplateSections.SelectMany(s => s.Fields))
            {
                propertyNames[templateField.Id] = templateField.PropertyName;
            }

            TemplateSections.Clear();

            Name = element.GetAttributeValue("name");
            ClassName = Name.GetSafeCodeIdentifier();

            foreach (var sectionElement in element.Elements())
            {
                var templateSection = ShapeCreator.CreateTemplateSection(ShapeCreator);
                TemplateSections.Add(templateSection);

                templateSection.Parse(sectionElement);
            }

            if (!string.IsNullOrEmpty(className))
            {
                ClassName = className;
            }

            foreach (var templateField in TemplateSections.SelectMany(s => s.Fields))
            {
                string propertyName;
                if (propertyNames.TryGetValue(templateField.Id, out propertyName))
                {
                    templateField.PropertyName = propertyName;
                }
            }
        }

        public virtual void Save([NotNull] XmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            output.WriteStartElement("template");

            SaveFields(output);

            foreach (var section in TemplateSections)
            {
                section.Save(output);
            }

            output.WriteEndElement();
        }

        protected virtual void SaveFields(XmlTextWriter output)
        {
            output.WriteAttributeString("templatename", Name);
            output.WriteAttributeString("templateid", TemplateUri.ItemId.ToString());
            output.WriteAttributeString("templateuri", TemplateUri.ToString());
            output.WriteAttributeString("classname", ClassName);
        }
    }
}
