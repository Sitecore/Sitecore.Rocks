// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Extensions.XmlNodeExtensions
{
    public static class XmlNodeExtensions
    {
        [NotNull]
        public static string GetAttributeValue([CanBeNull] this XmlNode node, [NotNull, Localizable(false)] string name)
        {
            Assert.ArgumentNotNull(name, nameof(name));

            if (node == null)
            {
                return string.Empty;
            }

            var attributes = node.Attributes;
            if (attributes == null)
            {
                return string.Empty;
            }

            var attribute = attributes[name];
            if (attribute == null)
            {
                return string.Empty;
            }

            return attribute.Value ?? string.Empty;
        }

        public static void SetAttributeValue([CanBeNull] this XmlNode node, [NotNull] string name, [NotNull] string value)
        {
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(value, nameof(value));

            if (node == null)
            {
                return;
            }

            var attributes = node.Attributes;
            if (attributes == null)
            {
                return;
            }

            var attribute = attributes[name];
            if (attribute == null)
            {
                var ownerDocument = node.OwnerDocument;
                if (ownerDocument == null)
                {
                    return;
                }

                attribute = ownerDocument.CreateAttribute(name);

                attributes.Append(attribute);
            }

            attribute.Value = value;
        }
    }
}
