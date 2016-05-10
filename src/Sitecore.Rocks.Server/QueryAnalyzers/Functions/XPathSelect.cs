// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml.Linq;
using System.Xml.XPath;
using Sitecore.Data.Query;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("XPathSelect", ShortHelp = "Performs an XPath selection against a complex field", LongHelp = "Performs an XPath selection against complex fields like layouts.", Example = "select XPathSelect(@__renderings, '/r/d', 'id') from /*/*")]
    public class XPathSelect : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            if (args.Arguments.Length <= 1 || args.Arguments.Length >= 4)
            {
                return null;
            }

            var xml = args.Arguments[0].Evaluate(args.Query, args.ContextNode) as string;
            var select = args.Arguments[1].Evaluate(args.Query, args.ContextNode) as string;

            if (string.IsNullOrEmpty(select) || string.IsNullOrEmpty(xml))
            {
                return string.Empty;
            }

            var attributeName = string.Empty;
            if (args.Arguments.Length == 3)
            {
                attributeName = args.Arguments[2].Evaluate(args.Query, args.ContextNode) as string;
                if (string.IsNullOrEmpty(attributeName))
                {
                    return string.Empty;
                }
            }

            try
            {
                var doc = XDocument.Parse(xml);

                var root = doc.Root;
                if (root == null)
                {
                    return string.Empty;
                }

                var element = root.XPathSelectElement(select);
                if (element == null)
                {
                    return string.Empty;
                }

                if (string.IsNullOrEmpty(attributeName))
                {
                    return element.Value;
                }

                if (!element.HasAttributes)
                {
                    return string.Empty;
                }

                var attribute = element.Attribute(attributeName);
                if (attribute == null)
                {
                    return string.Empty;
                }

                return attribute.Value;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
