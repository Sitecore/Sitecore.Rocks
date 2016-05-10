// © 2015 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using System.Xml.Linq;

namespace Sitecore.Extensions.XElementExtensions
{
    public static class XElementExtensions
    {
        public static string GetAttributeValue(this XElement element, [Localizable(false)] string attributeName, string defaultValue = "")
        {
            if (!element.HasAttributes)
            {
                return defaultValue;
            }

            var attribute = element.Attribute(attributeName);

            return attribute == null ? defaultValue : attribute.Value;
        }

        public static string GetElementValue(this XElement element, [Localizable(false)] string elementName, string defaultValue = "")
        {
            var e = element.Element(elementName);

            return e == null ? defaultValue : e.Value;
        }
    }
}
