// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers
{
    public static class QueryAnalyzer
    {
        [CanBeNull]
        public static object Evaluate([NotNull] string script, [NotNull] Item contextItem, int max)
        {
            Assert.ArgumentNotNull(script, nameof(script));
            Assert.ArgumentNotNull(contextItem, nameof(contextItem));

            var opcode = QueryAnalyzerParser.ParseScript(script);

            var query = new QueryAnalyzerQuery(opcode);

            query.Function += FunctionCall;

            if (max >= 0)
            {
                query.Max = max;
            }

            return query.Execute(contextItem);
        }

        [CanBeNull]
        public static object FunctionCall([NotNull] FunctionArgs args)
        {
            Assert.ArgumentNotNull(args, nameof(args));

            var function = QueryAnalyzerManager.Functions.FirstOrDefault(f => string.Compare(f.Attribute.FunctionName, args.FunctionName, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (function == null)
            {
                return Data.Query.Functions.FunctionCall(args);
            }

            return function.Function.Invoke(args);
        }
    }
}
