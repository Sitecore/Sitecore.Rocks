// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("insert", QueryAnalyzerTokenType.Insert, ShortHelp = "Inserts items", LongHelp = "Creates new items and adds them to the database. The columns @@name, @@templateitem and @@path must be supplied. " + "The @@itemname value must evaluate to a string value and\n" + "the @@templateitem and @@path values must evaluate to a single item.", Syntax = "'insert' 'into' '(' Fields ')' 'values' '(' Values ')'\n" + "    Fields = Field | (Fields ',' Field)\n" + "    Field = Attribute | '@@itemname' | '@@templateitem' | '@@path'\n" + "    Values = Expression | (Values ',' Expression)", Example = "insert into (@@itemname, @@templateitem, @@path, @Text)\n" + "values (\"Hola\", /sitecore/templates/sample/sample item, /sitecore/content/Home, \"Hello World\")"), ReservedWord("values", QueryAnalyzerTokenType.Values), ReservedWord("into", QueryAnalyzerTokenType.Into)]
    public class InsertKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));

            parser.Match(QueryAnalyzerTokenType.Insert, "\"insert\" expected");
            parser.Match(QueryAnalyzerTokenType.Into, "\"into\" expected");

            parser.Match(TokenType.StartParentes, "'(' expected");

            var columns = GetInsertColumns(parser);

            parser.Match(TokenType.EndParentes, "')' expected");

            parser.Match(QueryAnalyzerTokenType.Values, "\"values\" expected");

            parser.Match(TokenType.StartParentes, "'(' expected");

            var values = GetInsertValues(parser);

            parser.Match(TokenType.EndParentes, "')' expected");

            if (columns.Count != values.Count)
            {
                parser.Raise("Number of columns and number of values do not match");
            }

            return new Insert(columns, values);
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
        private List<string> GetInsertColumns([NotNull] Parser parser)
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
        private List<Opcode> GetInsertValues([NotNull] Parser parser)
        {
            Debug.ArgumentNotNull(parser, nameof(parser));

            var result = new List<Opcode>();

            result.Add(parser.GetExpression());

            while (parser.Token.Type == TokenType.Comma)
            {
                parser.Match();

                result.Add(parser.GetExpression());
            }

            return result;
        }
    }
}
