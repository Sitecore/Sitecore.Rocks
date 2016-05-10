// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml.Linq;
using Sitecore.Data.Query;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("Attribute", ShortHelp = "Gets an Xml attribute of an Xml-based field.", LongHelp = "Gets an xml attribute of complex fields like Image and Links.", Example = "select Attribute(@__image, 'alt') from /sitecore/content/Html")]
    public class Attribute : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            if (args.Arguments.Length != 2)
            {
                return null;
            }

            var xml = args.Arguments[0].Evaluate(args.Query, args.ContextNode) as string;
            var name = args.Arguments[1].Evaluate(args.Query, args.ContextNode) as string;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(xml))
            {
                return string.Empty;
            }

            try
            {
                var doc = XDocument.Parse(xml);

                var root = doc.Root;
                if (root == null)
                {
                    return string.Empty;
                }

                if (!root.HasAttributes)
                {
                    return string.Empty;
                }

                var attribute = root.Attribute(name);
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
