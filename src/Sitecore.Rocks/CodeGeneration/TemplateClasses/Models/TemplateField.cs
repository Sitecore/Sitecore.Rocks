// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.CodeGeneration.TemplateClasses.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers), DisplayName(@"Template Field"), DefaultProperty("Name"), Description("Template Section")]
    public class TemplateField
    {
        [NotNull, Browsable(false)]
        public string FormattedName
        {
            get { return "    " + Name; }
        }

        [ReadOnly(true), Category("Template Field"), Description("The ID of the template field."), DisplayName("Template Field ID")]
        public string Id { get; set; }

        [ReadOnly(true), Category("Template Field"), Description("The name of the template field."), DisplayName("Template Field Name")]
        public string Name { get; set; }

        [Category("Code Generation"), Description("The name of the property in the generated class."), DisplayName("Property Name")]
        public string PropertyName { get; set; }

        [Browsable(false)]
        public TemplateSection Section { get; set; }

        [ReadOnly(true), Category("Attributes"), Description("Shared flag.")]
        public bool Shared { get; set; }

        [ReadOnly(true), Category("Template Field"), Description("The data source.")]
        public string Source { get; set; }

        [ReadOnly(true), Category("Template Field"), Description("The field type.")]
        public string Type { get; set; }

        [ReadOnly(true), Category("Attributes"), Description("Unversioned flag.")]
        public bool Unversioned { get; set; }

        public virtual void Load([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            Id = element.GetAttributeValue("id");
            Name = element.GetAttributeValue("name");
            Shared = element.GetAttributeValue("shared") == "1";
            Source = element.GetAttributeValue("source");
            Type = element.GetAttributeValue("type");
            Unversioned = element.GetAttributeValue("unversioned") == "1";
            PropertyName = element.GetAttributeValue("propertyname");
        }

        public virtual void Parse([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            Id = element.GetAttributeValue("id");
            Name = element.GetAttributeValue("name");
            Type = element.GetAttributeValue("type");
            Source = element.GetAttributeValue("source");
            Shared = element.GetAttributeValue("shared") == @"1";
            Unversioned = element.GetAttributeValue("unversioned") == @"1";

            if (string.IsNullOrEmpty(Type))
            {
                Type = "Single-Line Text";
            }

            PropertyName = Name.GetSafeCodeIdentifier();
        }

        public virtual void Save([NotNull] XmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            output.WriteStartElement("field");

            SaveFields(output);

            output.WriteEndElement();
        }

        protected virtual void SaveFields(XmlTextWriter output)
        {
            output.WriteAttributeString("id", Id);
            output.WriteAttributeString("name", Name);
            output.WriteAttributeString("shared", Shared ? "1" : "0");
            output.WriteAttributeString("source", Source);
            output.WriteAttributeString("type", Type);
            output.WriteAttributeString("unversioned", Unversioned ? "1" : "0");
            output.WriteAttributeString("propertyname", PropertyName);
        }
    }
}
