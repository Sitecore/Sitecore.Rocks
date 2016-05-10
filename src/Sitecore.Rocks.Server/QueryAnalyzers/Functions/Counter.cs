// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("Counter", ShortHelp = "An auto incrementing counter", LongHelp = "A counter that starts at zero and increments every time it is evaluated.", Example = "select @Text + Counter() from /*/*")]
    public class Counter : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            var q = args.Query as QueryAnalyzerQuery;
            if (q == null)
            {
                return "0";
            }

            var value = q.Counter;
            q.Counter++;

            if (args.Arguments.Length == 0)
            {
                return value.ToString();
            }

            var format = args.Arguments[0].Evaluate(args.Query, args.ContextNode) as string;
            if (string.IsNullOrEmpty(format))
            {
                return value.ToString();
            }

            return value.ToString(format);
        }
    }
}
