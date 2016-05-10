// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("GetPath", ShortHelp = "Returns the path of the provided expression", LongHelp = "Returns the path of the specified expression", Example = "select GetPath(.) from /*/*")]
    public class GetPath : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            if (args.Arguments.Length != 1)
            {
                return null;
            }

            var result = args.Arguments[0].Evaluate(args.Query, args.ContextNode);
            if (result == null)
            {
                return string.Empty;
            }

            var items = result as QueryContext[];
            if (items != null)
            {
                return string.Empty;
            }

            var item = result as QueryContext;
            if (item != null)
            {
                return item.GetQueryContextItem().Paths.Path;
            }

            var id = result as string;
            if (string.IsNullOrEmpty(id))
            {
                return string.Empty;
            }

            var i = args.ContextNode.GetQueryContextItem().Database.GetItem(id);

            return i == null ? string.Empty : i.Paths.Path;
        }
    }
}
