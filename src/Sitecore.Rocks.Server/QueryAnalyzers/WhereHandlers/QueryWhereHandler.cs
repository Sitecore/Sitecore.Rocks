// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.WhereHandlers
{
    [WhereHandler("query", QueryAnalyzerTokenType.Query)]
    public class QueryWhereHandler : IWhereHandler
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));

            parser.Match(QueryAnalyzerTokenType.Query, "\"query\" expected");

            return parser.GetQueries();
        }
    }
}
