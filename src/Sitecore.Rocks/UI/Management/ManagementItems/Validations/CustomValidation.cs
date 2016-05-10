// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls.QueryBuilders;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Extensions.XmlTextWriterExtensions;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations
{
    public class CustomValidation
    {
        public CustomValidation()
        {
            Type = CustomValidationType.Query;
            Category = string.Empty;
            Code = string.Empty;
            Fix = string.Empty;
            Problem = string.Empty;
            Solution = string.Empty;
            Title = string.Empty;
            FileName = string.Empty;
            WhenExists = true;
        }

        [NotNull]
        public string Category { get; set; }

        [NotNull]
        public string Code { get; set; }

        [NotNull]
        public string FileName { get; set; }

        [NotNull]
        public string Fix { get; set; }

        [NotNull]
        public string Problem { get; set; }

        public SeverityLevel Severity { get; set; }

        [NotNull]
        public string Solution { get; set; }

        [NotNull]
        public string Title { get; set; }

        public CustomValidationType Type { get; set; }

        public bool WhenExists { get; set; }

        public void Load([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            Type = (CustomValidationType)element.GetAttributeInt("type", 0);
            Severity = (SeverityLevel)element.GetAttributeInt("severity", 0);
            Code = element.GetElementValue("code");
            Category = element.GetElementValue("category");
            Title = element.GetElementValue("title");
            Problem = element.GetElementValue("problem");
            Solution = element.GetElementValue("solution");
            Fix = element.GetElementValue("fix");

            var exists = element.GetElementValue("whenexists");
            if (!string.IsNullOrEmpty(exists))
            {
                WhenExists = exists == "true";
            }
        }

        public void Save([NotNull] XmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            output.WriteStartElement("validation");

            output.WriteAttributeString("type", ((int)Type).ToString());
            output.WriteAttributeString("severity", ((int)Severity).ToString());

            output.WriteElementCData("code", Code);
            output.WriteElementString("category", Category);
            output.WriteElementString("title", Title);
            output.WriteElementCData("problem", Problem);
            output.WriteElementCData("solution", Solution);
            output.WriteElementCData("fix", Fix);
            output.WriteElementCData("whenexists", WhenExists ? "true" : "false");

            output.WriteEndElement();
        }
    }
}
