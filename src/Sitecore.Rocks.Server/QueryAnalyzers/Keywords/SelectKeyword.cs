// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("select", QueryAnalyzerTokenType.Select, ShortHelp = "Performs a query", LongHelp = "Performs a query and displays the results in a grid.\n\nThe Order By clause operates on items - not on selected columns.", Syntax = "'select' ['distinct'] ('*' | Fields) ['from' Expression] [order by FieldNames]\n" + "    Fields = Field | (Fields ',' Field)\n" + "    Field = Expression ['as' Identifier]\n" + "    FieldNames = FieldName ['asc' | 'desc'] | (FieldNames ',' FieldName)", Example = "select * from //*[@@templatename='Sample Item'];\n" + "select @Text + \" text\" as TextField from /sitecore/content//* order by @Text desc"), ReservedWord("from", QueryAnalyzerTokenType.From), ReservedWord("search", QueryAnalyzerTokenType.Search), ReservedWord("as", QueryAnalyzerTokenType.As), ReservedWord("order", QueryAnalyzerTokenType.Order), ReservedWord("by", QueryAnalyzerTokenType.By), ReservedWord("asc", QueryAnalyzerTokenType.Asc), ReservedWord("desc", QueryAnalyzerTokenType.Desc), ReservedWord("distinct", QueryAnalyzerTokenType.Distinct)]
    public class SelectKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));

            parser.Match(QueryAnalyzerTokenType.Select, "\"select\" expected");

            var isDistinct = false;
            if (parser.Token.Type == QueryAnalyzerTokenType.Distinct)
            {
                parser.Match();
                isDistinct = true;
            }

            var selectFields = GetSelectColumns(parser);

            Opcode from = null;
            if (parser.Token.Type == QueryAnalyzerTokenType.From)
            {
                from = parser.GetFrom();
            }

            IEnumerable<OrderByColumn> orderBy = null;
            if (parser.Token.Type == QueryAnalyzerTokenType.Order)
            {
                orderBy = GetOrderBy(parser, selectFields);
            }

            return new Select(selectFields, from, orderBy, isDistinct);
        }

        [NotNull]
        private IEnumerable<OrderByColumn> GetOrderBy([NotNull] Parser parser, [NotNull] List<ColumnExpression> selectFields)
        {
            Debug.ArgumentNotNull(parser, nameof(parser));

            parser.Match(QueryAnalyzerTokenType.Order, "\"order\" expected");
            parser.Match(QueryAnalyzerTokenType.By, "\"by\" expected");

            var columns = new List<OrderByColumn>();

            var column = GetOrderByColumn(parser);
            columns.Add(column);

            while (parser.Token.Type == TokenType.Comma)
            {
                parser.Match();

                column = GetOrderByColumn(parser);
                columns.Add(column);
            }

            for (var index0 = 0; index0 < columns.Count; index0++)
            {
                var column0 = columns[index0];
                var column0Name = column0.ColumnName;

                if (!selectFields.Any(f => f.ColumnName == column0Name))
                {
                    parser.Raise(string.Format("Order By column \"{0}\" is not part of the selection", column0Name));
                }

                for (var index1 = index0 + 1; index1 < columns.Count; index1++)
                {
                    var column1 = columns[index1];

                    if (column1.ColumnName == column0Name)
                    {
                        parser.Raise(string.Format("Order By column \"{0}\" is already specified", column1.ColumnName));
                    }
                }
            }

            return columns;
        }

        [NotNull]
        private OrderByColumn GetOrderByColumn([NotNull] Parser parser)
        {
            Debug.ArgumentNotNull(parser, nameof(parser));

            var columnName = parser.Token.Value;
            parser.Match(TokenType.Identifier, "Column name expected (do not include @)");

            var direction = 1;

            if (parser.Token.Type == QueryAnalyzerTokenType.Asc)
            {
                parser.Match();
                direction = 1;
            }
            else if (parser.Token.Type == QueryAnalyzerTokenType.Desc)
            {
                parser.Match();
                direction = -1;
            }

            return new OrderByColumn(columnName, direction);
        }

        [NotNull]
        private ColumnExpression GetSelectColumn([NotNull] Parser parser)
        {
            Debug.ArgumentNotNull(parser, nameof(parser));

            var result = new ColumnExpression
            {
                Expression = parser.GetExpression()
            };

            var fieldExpression = result.Expression as FieldElement;
            if (fieldExpression != null)
            {
                if (string.Compare(fieldExpression.Name, "@path", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    result.ColumnName = "Path";
                    result.FieldName = null;
                    result.Expression = new ItemPath();
                }
                else if (string.Compare(fieldExpression.Name, "@version", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    result.ColumnName = "Version";
                    result.FieldName = null;
                    result.Expression = new ItemVersion();
                }
                else if (string.Compare(fieldExpression.Name, "@language", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    result.ColumnName = "Language";
                    result.FieldName = null;
                    result.Expression = new ItemLanguage();
                }
                else if (string.Compare(fieldExpression.Name, "@database", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    result.ColumnName = "Database";
                    result.FieldName = null;
                    result.Expression = new ItemDatabase();
                }
                else if (string.Compare(fieldExpression.Name, "@shortid", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    result.ColumnName = "ShortId";
                    result.FieldName = null;
                    result.Expression = new ItemShortId();
                }
                else if (!fieldExpression.Name.StartsWith("@"))
                {
                    result.ColumnName = fieldExpression.Name;
                    result.FieldName = fieldExpression.Name;
                    result.Expression = null;
                }
            }

            if (parser.Token.Type == QueryAnalyzerTokenType.As)
            {
                parser.Match();

                result.ColumnName = parser.Token.Value;
                parser.Match(TokenType.Identifier, "Identifier expected");
            }

            return result;
        }

        [NotNull]
        private List<ColumnExpression> GetSelectColumns([NotNull] Parser parser)
        {
            Debug.ArgumentNotNull(parser, nameof(parser));

            var columnExpressions = new List<ColumnExpression>();

            if (parser.Token.Type == TokenType.Star)
            {
                GetStarColumns(parser, columnExpressions);
                return columnExpressions;
            }

            var selectField = GetSelectColumn(parser);

            var columnName = selectField.ColumnName;
            if (columnExpressions.Any(c => c.ColumnName == columnName))
            {
                throw new ParseException(string.Format("Column \"{0}\" is already defined. Use the \"as\" operator to give a column a new name.", columnName));
            }

            columnExpressions.Add(selectField);

            while (parser.Token.Type == TokenType.Comma)
            {
                parser.Match();

                selectField = GetSelectColumn(parser);

                var name = selectField.ColumnName;
                if (columnExpressions.Any(c => c.ColumnName == name))
                {
                    throw new ParseException(string.Format("Column \"{0}\" is already defined. Use the \"as\" operator to give a column a new name.", name));
                }

                columnExpressions.Add(selectField);
            }

            return columnExpressions;
        }

        private void GetStarColumns([NotNull] Parser parser, [NotNull] List<ColumnExpression> columnExpressions)
        {
            Debug.ArgumentNotNull(parser, nameof(parser));
            Debug.ArgumentNotNull(columnExpressions, nameof(columnExpressions));

            parser.Match();

            columnExpressions.Add(new ColumnExpression
            {
                ColumnName = "Name",
                Expression = new FieldElement("@name")
            });

            columnExpressions.Add(new ColumnExpression
            {
                ColumnName = "ID",
                Expression = new FieldElement("@id")
            });

            columnExpressions.Add(new ColumnExpression
            {
                ColumnName = "Template Name",
                Expression = new FieldElement("@templatename")
            });

            columnExpressions.Add(new ColumnExpression
            {
                ColumnName = "Path",
                Expression = new ItemPath()
            });
        }
    }
}
