// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("replace", QueryAnalyzerTokenType.Replace, ShortHelp = "Search and replace strings in field values", LongHelp = "Performs a search and replace operation on field values", Syntax = "'replace' Expression 'with' Expression ['in' FieldName] ['from' Expression]\n", Example = "replace \"SiteCore\" with \"Sitecore\" in Title from /sitecore/content//*[@@templatename = \"Sample Item\"]"), ReservedWord("with", QueryAnalyzerTokenType.With), ReservedWord("in", QueryAnalyzerTokenType.In)]
    public class ReplaceKeyword : IQueryAnalyzerKeyword
    {
        [CanBeNull]
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));

            parser.Match(QueryAnalyzerTokenType.Replace, "\"replace\" expected");

            var find = parser.GetExpression();

            parser.Match(QueryAnalyzerTokenType.With, "\"with\" expected");

            var with = parser.GetExpression();

            string field = null;
            if (parser.Token.Type == QueryAnalyzerTokenType.In)
            {
                parser.Match(QueryAnalyzerTokenType.In, "\"in\" expected");
                field = GetFieldName(parser);
            }

            Opcode from = null;
            if (parser.Token.Type == QueryAnalyzerTokenType.From)
            {
                from = parser.GetFrom();
            }

            return new Replace(find, with, field, from);
        }

        [NotNull]
        private string GetFieldName([NotNull] Parser parser)
        {
            Debug.ArgumentNotNull(parser, nameof(parser));

            var expression = parser.GetAttribute();

            var fieldElement = expression as FieldElement;
            if (fieldElement != null)
            {
                return fieldElement.Name;
            }

            parser.Raise("Field name expected");
            return string.Empty;
        }
    }
}
