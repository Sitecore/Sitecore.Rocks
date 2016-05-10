// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("VersionCount", ShortHelp = "Returns number of versions of the current item", LongHelp = "Returns number of versions in the current language of the current item.", Example = "select VersionCount(.) from /*/*")]
    public class VersionCount : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            if (args.Arguments.Length != 1)
            {
                return null;
            }

            var queryContext = args.Arguments[0].Evaluate(args.Query, args.ContextNode) as QueryContext;
            if (queryContext == null)
            {
                return -1;
            }

            var item = queryContext.GetQueryContextItem();
            if (item == null)
            {
                return -1;
            }

            return item.Versions.Count;
        }
    }
}
