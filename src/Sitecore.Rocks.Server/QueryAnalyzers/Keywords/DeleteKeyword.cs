// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("delete", QueryAnalyzerTokenType.Delete, ShortHelp = "Deletes items", LongHelp = "Deletes items from the database.", Syntax = "'delete' ['from' Expression]", Example = "delete from /sitecore/content//*[@@templatename='Sample Item']")]
    public class DeleteKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));

            parser.Match(QueryAnalyzerTokenType.Delete, "\"delete\" expected");

            Opcode from = null;
            if (parser.Token.Type == QueryAnalyzerTokenType.From)
            {
                from = parser.GetFrom();
            }

            return new Delete(from);
        }
    }
}
