// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.WhereHandlers
{
    [WhereHandler("fastquery", QueryAnalyzerTokenType.FastQuery)]
    public class FastQueryWhereHandler : IWhereHandler
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));

            parser.Match(QueryAnalyzerTokenType.FastQuery, "\"fastquery\" expected");

            var literal = parser.Token.Value;
            parser.Match(TokenType.Literal, "Literal expected");

            return new FastQuery(literal);
        }
    }
}
