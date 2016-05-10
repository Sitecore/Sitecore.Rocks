// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Extensions.XElementExtensions
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

        public static DateTime GetAttributeDateTime([NotNull] this XElement element, [NotNull, Localizable(false)] string attributeName, DateTime defaultValue)
        {
            Assert.ArgumentNotNull(element, nameof(element));
            Assert.ArgumentNotNull(attributeName, nameof(attributeName));

            var value = GetAttributeValue(element, attributeName);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            DateTime result;
            if (DateTime.TryParse(value, out result))
            {
                return result;
            }

            return defaultValue;
        }

        public static double GetAttributeDouble([NotNull] this XElement element, [NotNull, Localizable(false)] string attributeName, double defaultValue)
        {
            Assert.ArgumentNotNull(element, nameof(element));
            Assert.ArgumentNotNull(attributeName, nameof(attributeName));

            var value = GetAttributeValue(element, attributeName);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            double result;
            if (double.TryParse(value, out result))
            {
                return result;
            }

            return defaultValue;
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

        public static DateTime GetAttributeIsoDateTime([NotNull] this XElement element, [NotNull, Localizable(false)] string attributeName, DateTime defaultValue)
        {
            Assert.ArgumentNotNull(element, nameof(element));
            Assert.ArgumentNotNull(attributeName, nameof(attributeName));

            var value = GetAttributeValue(element, attributeName);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            return DateTimeExtensions.DateTimeExtensions.FromIso(value, defaultValue);
        }

        public static long GetAttributeLong([NotNull] this XElement element, [NotNull, Localizable(false)] string attributeName, long defaultValue)
        {
            Assert.ArgumentNotNull(element, nameof(element));
            Assert.ArgumentNotNull(attributeName, nameof(attributeName));

            var value = GetAttributeValue(element, attributeName);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            long result;
            if (long.TryParse(value, out result))
            {
                return result;
            }

            return defaultValue;
        }

        [NotNull]
        public static string GetAttributeValue([NotNull] this XElement element, [NotNull, Localizable(false)] string attributeName)
        {
            Assert.ArgumentNotNull(element, nameof(element));
            Assert.ArgumentNotNull(attributeName, nameof(attributeName));

            if (!element.HasAttributes)
            {
                return string.Empty;
            }

            var attribute = element.Attribute(attributeName);

            return attribute == null ? string.Empty : attribute.Value;
        }

        [CanBeNull]
        public static string GetAttributeValue([NotNull] this XElement element, [NotNull, Localizable(false)] string attributeName, [CanBeNull] string defaultValue)
        {
            Assert.ArgumentNotNull(element, nameof(element));
            Assert.ArgumentNotNull(attributeName, nameof(attributeName));

            if (!element.HasAttributes)
            {
                return defaultValue;
            }

            var attribute = element.Attribute(attributeName);

            return attribute == null ? defaultValue : attribute.Value;
        }

        [NotNull]
        public static string GetElementValue([NotNull] this XElement element, [NotNull, Localizable(false)] string elementName)
        {
            Assert.ArgumentNotNull(element, nameof(element));
            Assert.ArgumentNotNull(elementName, nameof(elementName));

            return GetElementValue(element, elementName, string.Empty);
        }

        [NotNull]
        public static string GetElementValue([NotNull] this XElement element, [NotNull, Localizable(false)] string elementName, [NotNull] string defaultValue)
        {
            Assert.ArgumentNotNull(element, nameof(element));
            Assert.ArgumentNotNull(elementName, nameof(elementName));
            Assert.ArgumentNotNull(defaultValue, nameof(defaultValue));

            var e = element.Element(elementName);

            return e == null ? defaultValue : e.Value;
        }

        [NotNull]
        public static int GetElementValueInt([NotNull] this XElement element, [NotNull, Localizable(false)] string elementName, int defaultValue = 0)
        {
            Assert.ArgumentNotNull(element, nameof(element));
            Assert.ArgumentNotNull(elementName, nameof(elementName));

            var e = element.Element(elementName);
            if (e == null || string.IsNullOrEmpty(e.Value))
            {
                return defaultValue;
            }

            int result;
            if (int.TryParse(e.Value, out result))
            {
                return result;
            }

            return defaultValue;
        }

        public static bool HasAttribute([NotNull] this XElement element, [NotNull, Localizable(false)] string attributeName)
        {
            Assert.ArgumentNotNull(element, nameof(element));
            Assert.ArgumentNotNull(attributeName, nameof(attributeName));

            if (!element.HasAttributes)
            {
                return false;
            }

            return element.Attribute(attributeName) != null;
        }

        [NotNull]
        public static string InnerText([NotNull] this XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            return string.Join(string.Empty, element.Nodes().Select(n => n.ToString()).ToArray());
        }
    }
}
