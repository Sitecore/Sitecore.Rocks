// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.WhereHandlers
{
    [WhereHandler("search", QueryAnalyzerTokenType.Search)]
    public class SearchWhereHandler : IWhereHandler
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));

            parser.Match(QueryAnalyzerTokenType.Search, "\"search\" expected");

            var literal = parser.Token.Value;
            parser.Match(TokenType.Literal, "Literal expected");

            return new Opcodes.Search(literal);
        }
    }
}
