// © 2015 Sitecore Corporation A/S. All rights reserved.

using System.Xml.Linq;

namespace Sitecore.Extensions.StringExtensions
{
    public static class StringExtensions
    {
        public static XElement ToXElement(this string text, LoadOptions loadOptions = LoadOptions.None)
        {
            XDocument doc;
            try
            {
                doc = XDocument.Parse(text, loadOptions);
            }
            catch
            {
                return null;
            }

            return doc.Root;
        }
    }
}
