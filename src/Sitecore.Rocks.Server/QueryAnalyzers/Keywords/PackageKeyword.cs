// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("package", QueryAnalyzerTokenType.Package, ShortHelp = "Packages items", LongHelp = "Creates a Sitecore package file from the selected items.", Syntax = "'package' [FileName] ['from' Expression]", Example = "package 'c:\\package.zip' from /sitecore/content//*[@@templatename='Sample Item']")]
    public class PackageKeyword : IQueryAnalyzerKeyword
    {
        [CanBeNull]
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));

            parser.Match(QueryAnalyzerTokenType.Package, "\"package\" expected");

            var fileName = parser.Token.Value;
            parser.Match(TokenType.Literal, "Literal (FileName) expected");

            Opcode from = null;
            if (parser.Token.Type == QueryAnalyzerTokenType.From)
            {
                from = parser.GetFrom();
            }

            return new Package(fileName, from);
        }
    }
}
