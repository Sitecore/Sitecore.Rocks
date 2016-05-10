// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("Paths", ShortHelp = "Returns the paths as a single string of the specified expression", LongHelp = "Returns the paths as a single string of the specified expression.", Example = "select Paths(.) from /*/*")]
    public class Paths : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            if (args.Arguments.Length != 1)
            {
                return null;
            }

            var item = args.ContextNode.GetQueryContextItem();
            if (item == null)
            {
                return string.Empty;
            }

            var list = args.Arguments[0].Evaluate(args.Query, args.ContextNode) as string;
            if (string.IsNullOrEmpty(list))
            {
                return string.Empty;
            }

            var result = string.Empty;

            foreach (var id in list.Split('|'))
            {
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                var i = item.Database.GetItem(id);
                if (i == null)
                {
                    result += id + "\r\n";
                }
                else
                {
                    result += i.Paths.Path + "\r\n";
                }
            }

            return result.Trim();
        }
    }
}
