// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.Publishing.Dialogs
{
    public class PublishProfile
    {
        public PublishProfile()
        {
            Name = string.Empty;

            Languages = new List<string>();
            Targets = new List<string>();
        }

        [NotNull]
        public List<string> Languages { get; }

        public int Mode { get; set; }

        [NotNull]
        public string Name { get; set; }

        public bool RelatedItems { get; set; }

        public int Source { get; set; }

        [NotNull]
        public List<string> Targets { get; }

        public void Load([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            Name = element.GetAttributeValue("name");
            Source = element.GetAttributeInt("source", 0);
            Mode = element.GetAttributeInt("mode", 0);
            RelatedItems = string.Compare(element.GetAttributeValue("related"), "true", StringComparison.InvariantCultureIgnoreCase) == 0;

            var languages = element.Element("languages");
            if (languages != null)
            {
                foreach (var language in languages.Elements().Select(l => l.GetAttributeValue("name")))
                {
                    Languages.Add(language);
                }
            }

            var targets = element.Element("targets");
            if (targets != null)
            {
                foreach (var target in targets.Elements().Select(t => t.GetAttributeValue("name")))
                {
                    Targets.Add(target);
                }
            }
        }

        public void Save([NotNull] XmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            output.WriteStartElement("profile");
            output.WriteAttributeString("name", Name);
            output.WriteAttributeString("source", Source.ToString(CultureInfo.InvariantCulture));
            output.WriteAttributeString("mode", Mode.ToString(CultureInfo.InvariantCulture));
            output.WriteAttributeString("related", RelatedItems ? "true" : "false");

            output.WriteStartElement("languages");

            foreach (var language in Languages)
            {
                output.WriteStartElement("language");
                output.WriteAttributeString("name", language);
                output.WriteEndElement();
            }

            output.WriteEndElement();

            output.WriteStartElement("targets");

            foreach (var target in Targets)
            {
                output.WriteStartElement("target");
                output.WriteAttributeString("name", target);
                output.WriteEndElement();
            }

            output.WriteEndElement();

            output.WriteEndElement();
        }
    }
}
