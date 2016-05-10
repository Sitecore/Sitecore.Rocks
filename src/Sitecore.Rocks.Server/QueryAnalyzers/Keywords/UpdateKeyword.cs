// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("update", QueryAnalyzerTokenType.Update, ShortHelp = "Updates item fields", LongHelp = "Updates fields in items.", Syntax = "'update' ['set' | 'reset'] Fields ['from' Expression]\n" + "    Fields = Fields | (Fields ',' Field)\n" + "    Field = Attribute '=' Expression\n", Example = "update set @Text = \"Hello\" from /sitecore/content//*[@@templatename = \"Sample Item\"]"), ReservedWord("reset", QueryAnalyzerTokenType.Reset)]
    public class UpdateKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));

            parser.Match(QueryAnalyzerTokenType.Update, "\"update\" expected");

            if (parser.Token.Type == QueryAnalyzerTokenType.Reset)
            {
                parser.Match(QueryAnalyzerTokenType.Reset, "\"reset\" expected");

                var resetColumns = GetResetColumns(parser);

                Opcode from = null;
                if (parser.Token.Type == QueryAnalyzerTokenType.From)
                {
                    from = parser.GetFrom();
                }

                return new Reset(resetColumns, from);
            }
            else
            {
                parser.Match(QueryAnalyzerTokenType.Set, "\"set\" expected");

                var selectFields = GetUpdateColumns(parser);

                Opcode from = null;
                if (parser.Token.Type == QueryAnalyzerTokenType.From)
                {
                    from = parser.GetFrom();
                }

                return new Opcodes.Update(selectFields, from);
            }
        }

        [NotNull]
        private string GetColumnName([NotNull] Parser parser)
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

        [NotNull]
        private List<string> GetResetColumns([NotNull] Parser parser)
        {
            Debug.ArgumentNotNull(parser, nameof(parser));

            var result = new List<string>();

            result.Add(GetColumnName(parser));

            while (parser.Token.Type == TokenType.Comma)
            {
                parser.Match();
                result.Add(GetColumnName(parser));
            }

            return result;
        }

        [NotNull]
        private ColumnExpression GetUpdateColumn([NotNull] Parser parser)
        {
            Debug.ArgumentNotNull(parser, nameof(parser));

            var columnName = GetColumnName(parser);

            parser.Match(TokenType.Equal, "'=' expected");

            var expression = parser.GetExpression();

            var result = new ColumnExpression
            {
                ColumnName = columnName,
                Expression = expression
            };

            var fieldExpression = result.Expression as FieldElement;
            if (fieldExpression != null)
            {
                result.ColumnName = fieldExpression.Name;
                result.FieldName = fieldExpression.Name;
                result.Expression = null;
            }

            return result;
        }

        [NotNull]
        private List<ColumnExpression> GetUpdateColumns([NotNull] Parser parser)
        {
            Debug.ArgumentNotNull(parser, nameof(parser));

            var columnExpressions = new List<ColumnExpression>();

            var column = GetUpdateColumn(parser);
            columnExpressions.Add(column);

            while (parser.Token.Type == TokenType.Comma)
            {
                parser.Match();

                column = GetUpdateColumn(parser);
                columnExpressions.Add(column);
            }

            return columnExpressions;
        }
    }
}
