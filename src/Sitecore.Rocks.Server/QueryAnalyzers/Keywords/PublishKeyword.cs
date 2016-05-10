// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("publish", QueryAnalyzerTokenType.Publish, ShortHelp = "Publishes items", LongHelp = "Publishes items.", Syntax = "'publish' [PublishingTargets] ['from' Expression]\n" + "PublishingTargets = PublishingTarget | (PublishingTargets ',' PublishingTarget)\n" + "PublishingTargets = Identifier ", Example = "publish from /sitecore/content//*[@@templatename='Sample Item']\n" + "publish Internet")]
    public class PublishKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));

            parser.Match(QueryAnalyzerTokenType.Publish, "\"publish\" expected");

            var targets = new List<string>();

            if (parser.Token.Type == TokenType.Identifier)
            {
                GetTargets(parser, targets);
            }

            Opcode from = null;
            if (parser.Token.Type == QueryAnalyzerTokenType.From)
            {
                from = parser.GetFrom();
            }

            return new Publish(from, targets);
        }

        private void GetTargets(Parser parser, List<string> targets)
        {
            targets.Add(parser.Token.Value);
            parser.Match(TokenType.Identifier, "Literal expected.");

            while (parser.Token.Type == TokenType.Comma)
            {
                parser.Match();

                targets.Add(parser.Token.Value);
                parser.Match(TokenType.Identifier, "Literal expected.");
            }
        }
    }
}
