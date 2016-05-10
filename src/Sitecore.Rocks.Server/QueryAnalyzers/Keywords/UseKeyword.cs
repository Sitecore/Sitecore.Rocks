// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("use", QueryAnalyzerTokenType.Use, ShortHelp = "Use a specific database", LongHelp = "Sets a specific database to use for further querying.", Syntax = "'use' Database", Example = "use Master")]
    public class UseKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));

            parser.Match(QueryAnalyzerTokenType.Use, "\"use\" expected");

            var key = parser.Token.Value;
            parser.Match(TokenType.Identifier, "Identifier expected");

            return new Use(key);
        }
    }
}
