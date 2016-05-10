// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Query;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("ContainsAny", ShortHelp = "Determines if a part of a string appears in another string.", LongHelp = "Determines if a part of a pipe-separated string appears in another pipe-separated string.", Example = "select ContainsAny('1|2|3', '2|4') from /sitecore/content/Html -- returns true as 2 appears in both strings")]
    public class ContainsAny : IFunction
    {
        private static readonly char[] Separator = new[]
        {
            '|'
        };

        public object Invoke(FunctionArgs args)
        {
            if (args.Arguments.Length != 2)
            {
                return null;
            }

            var left = args.Arguments[0].Evaluate(args.Query, args.ContextNode) as string;
            var right = args.Arguments[1].Evaluate(args.Query, args.ContextNode) as string;
            if (string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right))
            {
                return false;
            }

            var l = new List<string>(left.Split(Separator, StringSplitOptions.RemoveEmptyEntries));
            var r = new List<string>(right.Split(Separator, StringSplitOptions.RemoveEmptyEntries));

            return l.Any(r.Contains);
        }
    }
}
