// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.CodeGeneration.TemplateClasses.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers), DisplayName(@"Template Section"), DefaultProperty("Name"), Description("Template Section")]
    public class TemplateSection
    {
        public TemplateSection([NotNull] IShapeCreator shapeCreator)
        {
            Assert.ArgumentNotNull(shapeCreator, nameof(shapeCreator));

            ShapeCreator = shapeCreator;
            Fields = new List<TemplateField>();
        }

        [Browsable(false)]
        public UI.TemplateDesigner.TemplateSection Control { get; set; }

        [Browsable(false)]
        public List<TemplateField> Fields { get; set; }

        [NotNull, Browsable(false)]
        public string FormattedName
        {
            get { return Name; }
        }

        [ReadOnly(true), Category("Template Section"), DisplayName("Template Section Id"), Description("The ID of the template section item.")]
        public string Id { get; set; }

        [ReadOnly(true), Category("Template Section"), DisplayName("Template Section Name"), Description("The name of the template section.")]
        public string Name { get; set; }

        [NotNull, Browsable(false)]
        public string Type
        {
            get { return string.Empty; }
        }

        protected IShapeCreator ShapeCreator { get; set; }

        public virtual void Load([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            Fields.Clear();

            Id = element.GetAttributeValue("id");
            Name = element.GetAttributeValue("name");

            foreach (var child in element.Elements())
            {
                var field = ShapeCreator.CreateTemplateField(ShapeCreator);

                field.Load(child);

                Fields.Add(field);
            }
        }

        public virtual void Parse([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            Id = element.GetAttributeValue("id");
            Name = element.GetAttributeValue("name");

            foreach (var field in element.Elements())
            {
                var templateField = ShapeCreator.CreateTemplateField(ShapeCreator);
                Fields.Add(templateField);

                templateField.Parse(field);
            }
        }

        public virtual void Save([NotNull] XmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            output.WriteStartElement("section");

            SaveFields(output);

            foreach (var field in Fields)
            {
                field.Save(output);
            }

            output.WriteEndElement();
        }

        protected virtual void SaveFields(XmlTextWriter output)
        {
            output.WriteAttributeString("id", Id);
            output.WriteAttributeString("name", Name);
        }
    }
}
