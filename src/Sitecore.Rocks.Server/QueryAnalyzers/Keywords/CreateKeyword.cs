// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("create", QueryAnalyzerTokenType.Create, ShortHelp = "Creates a template", LongHelp = "Creates a new template in the database.", Syntax = "'create' 'template' TemplateName Location '(' Fields ')'\n" + "    TemplateName = Identifier\n" + "    Location = Expression (* must evaluate to a single item *)\n" + "    Fields = Field | (Fields ',' Field)\n" + "    Field = (FieldName FieldType ['shared' | 'unversioned'] ['section' Identifier])", Example = "create template MyTemplate /sitecore/templates\n" + "(\n" + "  Title #Single-line Text# section Data,\n" + "  Value #Single-line Text# shared section Data\n" + ");"), ReservedWord("create", QueryAnalyzerTokenType.Create), ReservedWord("template", QueryAnalyzerTokenType.Template), ReservedWord("shared", QueryAnalyzerTokenType.Shared), ReservedWord("unversioned", QueryAnalyzerTokenType.Unversioned), ReservedWord("section", QueryAnalyzerTokenType.Section)]
    public class CreateKeyword : IQueryAnalyzerKeyword
    {
        [CanBeNull]
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));

            parser.Match(QueryAnalyzerTokenType.Create, "\"create\" expected");
            parser.Match(QueryAnalyzerTokenType.Template, "\"create\" expected");

            var templateName = parser.Token.Value;
            parser.Match(TokenType.Identifier, "Identifier expected");

            var location = parser.GetExpression();
            if (location == null)
            {
                throw new QueryException("Expected expression");
            }

            parser.Match(TokenType.StartParentes, "'(' expected");

            var createColumns = GetCreateColumns(parser);

            parser.Match(TokenType.EndParentes, "')' expected");

            return new Create(templateName, location, createColumns);
        }

        [NotNull]
        private CreateColumn GetCreateColumn([NotNull] Parser parser)
        {
            Debug.ArgumentNotNull(parser, nameof(parser));

            var result = new CreateColumn();

            result.FieldName = parser.Token.Value;
            parser.Match(TokenType.Identifier, "\"Identifier expected\"");

            result.FieldType = parser.Token.Value;
            parser.Match(TokenType.Identifier, "\"Identifier expected\"");

            switch (parser.Token.Type)
            {
                case QueryAnalyzerTokenType.Shared:
                    parser.Match();
                    result.Shared = true;
                    break;
                case QueryAnalyzerTokenType.Unversioned:
                    parser.Match();
                    result.Unversioned = true;
                    break;
            }

            if (parser.Token.Type == QueryAnalyzerTokenType.Section)
            {
                parser.Match();

                result.Section = parser.Token.Value;
                parser.Match(TokenType.Identifier, "\"Identifier expected\"");
            }

            return result;
        }

        [NotNull]
        private List<CreateColumn> GetCreateColumns([NotNull] Parser parser)
        {
            Debug.ArgumentNotNull(parser, nameof(parser));

            var columnExpressions = new List<CreateColumn>();

            var createColumn = GetCreateColumn(parser);
            columnExpressions.Add(createColumn);

            while (parser.Token.Type == TokenType.Comma)
            {
                parser.Match();

                createColumn = GetCreateColumn(parser);
                columnExpressions.Add(createColumn);
            }

            return columnExpressions;
        }

        public class CreateColumn
        {
            public string FieldName { get; set; }

            public string FieldType { get; set; }

            public string Section { get; set; }

            public bool Shared { get; set; }

            public bool Unversioned { get; set; }
        }
    }
}
