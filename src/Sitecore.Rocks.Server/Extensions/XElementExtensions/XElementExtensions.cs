// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Extensions.XElementExtensions
{
    public static class XElementExtensions
    {
        [CanBeNull]
        public static XElement Element([NotNull] this XElement element, int index)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            var elements = element.Elements();

            var count = 0;
            foreach (var e in elements)
            {
                if (count == index)
                {
                    return e;
                }

                count++;
            }

            return null;
        }

        public static int GetAttributeInt([NotNull] this XElement element, [NotNull, Localizable(false)] string attributeName, int defaultValue)
        {
            Assert.ArgumentNotNull(element, nameof(element));
            Assert.ArgumentNotNull(attributeName, nameof(attributeName));

            var value = GetAttributeValue(element, attributeName);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            int result;
            if (int.TryParse(value, out result))
            {
                return result;
            }

            return defaultValue;
        }

        [NotNull]
        public static string GetAttributeValue([NotNull] this XElement element, [NotNull] string name)
        {
            Assert.ArgumentNotNull(element, nameof(element));
            Assert.ArgumentNotNull(name, nameof(name));

            var attribute = element.Attribute(name);
            if (attribute == null)
            {
                return string.Empty;
            }

            return attribute.Value ?? string.Empty;
        }

        [CanBeNull]
        public static string GetAttributeValue([NotNull] this XElement element, [NotNull] string name, [CanBeNull] string defaultValue)
        {
            Assert.ArgumentNotNull(element, nameof(element));
            Assert.ArgumentNotNull(name, nameof(name));

            var attribute = element.Attribute(name);
            if (attribute == null)
            {
                return defaultValue;
            }

            return attribute.Value ?? defaultValue;
        }

        [NotNull]
        public static string GetElementCData([NotNull] this XElement element, [NotNull, Localizable(false)] string elementName)
        {
            Assert.ArgumentNotNull(element, nameof(element));
            Assert.ArgumentNotNull(elementName, nameof(elementName));

            var e = element.Element(elementName);
            if (e == null)
            {
                return string.Empty;
            }

            var cdata = e.Elements().FirstOrDefault();
            if (cdata == null)
            {
                return string.Empty;
            }

            return cdata.Value;
        }

        [NotNull]
        public static string GetElementValue([NotNull] this XElement element, [NotNull, Localizable(false)] string elementName)
        {
            Assert.ArgumentNotNull(element, nameof(element));
            Assert.ArgumentNotNull(elementName, nameof(elementName));

            var e = element.Element(elementName);

            return e == null ? string.Empty : e.Value;
        }

        [NotNull]
        public static string InnerText([NotNull] this XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            return string.Join(string.Empty, element.Nodes().Select(n => n.ToString()).ToArray());
        }
    }
}
