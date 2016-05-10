// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("serialize", QueryAnalyzerTokenType.Serialize, ShortHelp = "Serializes items", LongHelp = "Serializes items to the disk.", Syntax = "'serialize' ['from' Expression]", Example = "serialize from /sitecore/content//*[@#__Updated# >= '20110122T212500']")]
    public class SerializeKeyword : IQueryAnalyzerKeyword
    {
        [CanBeNull]
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));

            parser.Match(QueryAnalyzerTokenType.Serialize, "\"serialize\" expected");

            Opcode from = null;
            if (parser.Token.Type == QueryAnalyzerTokenType.From)
            {
                from = parser.GetFrom();
            }

            return new Serialize(from);
        }
    }
}
