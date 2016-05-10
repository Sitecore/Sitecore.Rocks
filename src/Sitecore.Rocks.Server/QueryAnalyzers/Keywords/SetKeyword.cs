// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("set", QueryAnalyzerTokenType.Set, ShortHelp = "Sets parameters", LongHelp = "Sets parameters.", Syntax = "'set' Setting '=' Expression\n" + "    Setting = 'contextnode' | 'maxitems'", Example = "set contextnode = /sitecore/content/home;\n" + "set maxitems = 999")]
    public class SetKeyword : IQueryAnalyzerKeyword
    {
        [CanBeNull]
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));

            parser.Match(QueryAnalyzerTokenType.Set, "\"set\" expected");

            var key = parser.Token.Value;
            parser.Match(TokenType.Identifier, "Identifier expected");
            parser.Match(TokenType.Equal, "'=' expected");

            switch (key.ToLower())
            {
                case "maxitems":
                    return SetMaxItems(parser);
                case "contextnode":
                    return SetContextNode(parser);
                case "language":
                    return SetLanguage(parser);
                default:
                    parser.Raise("Unknown key for Set keyword");
                    break;
            }

            return null;
        }

        [NotNull]
        private Opcode SetContextNode([NotNull] Parser parser)
        {
            Debug.ArgumentNotNull(parser, nameof(parser));

            var expression = parser.GetExpression();

            return new SetContextNode(expression);
        }

        [NotNull]
        private Opcode SetLanguage([NotNull] Parser parser)
        {
            Debug.ArgumentNotNull(parser, nameof(parser));

            var expression = parser.GetExpression();

            return new SetLanguage(expression);
        }

        [NotNull]
        private Opcode SetMaxItems([NotNull] Parser parser)
        {
            Debug.ArgumentNotNull(parser, nameof(parser));

            var maxItems = parser.Token.NumberValue;
            parser.Match(TokenType.Number, "Number expected");

            return new SetMaxItems(maxItems);
        }
    }
}
