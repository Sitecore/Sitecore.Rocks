// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Extensions.XmlTextWriterExtensions
{
    public static class XmlTextWriterExtensions
    {
        public static void WriteAttributeStringNotEmpty([NotNull] this XmlTextWriter xmlTextWriter, [NotNull] string localName, [CanBeNull] string value)
        {
            Assert.ArgumentNotNull(xmlTextWriter, nameof(xmlTextWriter));
            Assert.ArgumentNotNull(localName, nameof(localName));

            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            xmlTextWriter.WriteAttributeString(localName, value);
        }

        public static void WriteElementCData([NotNull] this XmlTextWriter xmlTextWriter, [NotNull] string elementName, [CanBeNull] string value)
        {
            Assert.ArgumentNotNull(xmlTextWriter, nameof(xmlTextWriter));
            Assert.ArgumentNotNull(elementName, nameof(elementName));

            xmlTextWriter.WriteStartElement(elementName);
            xmlTextWriter.WriteCData(value);
            xmlTextWriter.WriteEndElement();
        }
    }
}
