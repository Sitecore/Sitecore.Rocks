// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("help", QueryAnalyzerTokenType.Help, ShortHelp = "Displays the list of keywords. Use \"help Keyword\" to get help about a specific keyword.", LongHelp = "Either displays the list of keywords or displays details about a specific keyword.", Syntax = "help [Keyword]", Example = "help select")]
    public class HelpKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));

            parser.Match(QueryAnalyzerTokenType.Help, "\"help\" expected");

            var keyword = string.Empty;

            if (parser.Token.Type != QueryAnalyzerTokenType.Semicolon && parser.Token.Type != TokenType.End)
            {
                keyword = parser.Token.Value ?? string.Empty;
                parser.Match();
            }

            return new Help(keyword);
        }
    }
}
