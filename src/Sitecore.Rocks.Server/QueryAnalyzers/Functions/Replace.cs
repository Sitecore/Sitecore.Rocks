// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("ReplaceString", ShortHelp = "Replaces all occurrences of a specified string in a string, with another specified string.", LongHelp = "Replaces all occurrences of a specified string in a string, with another specified string.", Example = "update set @@name = replacestring(@@name, 'Home', 'My Home') from /sitecore/content/Html")]
    public class Replace : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            if (args.Arguments.Length != 3)
            {
                return null;
            }

            var str = args.Arguments[0].Evaluate(args.Query, args.ContextNode) as string;
            var replace = args.Arguments[1].Evaluate(args.Query, args.ContextNode) as string;
            var with = args.Arguments[2].Evaluate(args.Query, args.ContextNode) as string;

            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(replace) || with == null)
            {
                return str;
            }

            return str.Replace(replace, with);
        }
    }
}
