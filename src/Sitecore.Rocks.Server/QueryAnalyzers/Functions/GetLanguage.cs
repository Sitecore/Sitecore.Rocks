// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("GetLanguage", ShortHelp = "Returns the language of the current item", LongHelp = "Returns the language of the current item.", Example = "select GetLanguage() from /*/*")]
    public class GetLanguage : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            var item = args.ContextNode.GetQueryContextItem();
            if (item == null)
            {
                return null;
            }

            return item.Language.ToString();
        }
    }
}
