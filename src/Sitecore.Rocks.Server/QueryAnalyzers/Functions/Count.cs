// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("Count", ShortHelp = "Returns the number of elements", LongHelp = "Returns the numbers of elements in an expression.", Example = "select Count(/sitecore/content/Home//*) from /*")]
    public class Count : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            if (args.Arguments.Length == 0)
            {
                return null;
            }

            var result = args.Arguments[0].Evaluate(args.Query, args.ContextNode);
            if (result == null)
            {
                return 0;
            }

            var item = result as QueryContext;
            if (item != null)
            {
                return 1;
            }

            var items = result as QueryContext[];
            if (items != null)
            {
                return items.Length;
            }

            return 1;
        }
    }
}
