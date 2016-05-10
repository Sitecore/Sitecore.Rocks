// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Reflection;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Parsers
{
    public class QueryAnalyzerParser : QueryParser
    {
        private readonly Parser parser;

        private readonly FieldInfo tokenFieldInfo;

        private QueryAnalyzerBuilder builder;

        private QueryAnalyzerTokenizer tokenizer;

        public QueryAnalyzerParser()
        {
            tokenFieldInfo = typeof(QueryParser).GetField("Token", BindingFlags.NonPublic | BindingFlags.Instance);

            parser = new Parser
            {
                GetToken = () => CurrentToken,
                DoMatch = Match,
                DoMatchTokenType = Match,
                DoGetExpression = GetExpression,
                DoGetQueries = GetQueries,
                DoGetAttribute = GetAttribute,
                DoGetBlock = GetBlock,
                DoGetComment = GetComment,
                DoGetText = GetText
            };
        }

        protected Token CurrentToken
        {
            get { return (Token)tokenFieldInfo.GetValue(this); }
        }

        public static Opcode ParseScript([NotNull] string script)
        {
            Assert.ArgumentNotNull(script, nameof(script));

            var parser = new QueryAnalyzerParser();

            return parser.DoScriptParse(script);
        }

        [NotNull]
        protected virtual Opcode DoScriptParse([NotNull] string script)
        {
            Debug.ArgumentNotNull(script, nameof(script));

            Initialize(script);

            var result = GetBatches();

            Match(TokenType.End, "End of script expected");

            return result;
        }

        [CanBeNull]
        private Opcode GetBatch()
        {
            var opcode = QueryAnalyzerManager.GetKeyword(parser, CurrentToken.Type);
            if (opcode != null)
            {
                return opcode;
            }

            throw new ParseException("Keyword expected at position " + CurrentToken.Index + ".");
        }

        [NotNull]
        private Opcode GetBatches()
        {
            var batches = new List<Opcode>();

            var opcode = GetBatch();
            if (!(opcode is Comment))
            {
                batches.Add(opcode);
            }

            while (CurrentToken.Type == QueryAnalyzerTokenType.Semicolon || opcode is Comment)
            {
                Match();

                if (CurrentToken.Type == QueryAnalyzerTokenType.Semicolon)
                {
                    continue;
                }

                if (CurrentToken.Type == TokenType.End)
                {
                    break;
                }

                opcode = GetBatch();

                if (!(opcode is Comment))
                {
                    batches.Add(opcode);
                }
            }

            return new Batches(batches);
        }

        [NotNull]
        private string GetBlock()
        {
            return tokenizer.GetBlock();
        }

        [NotNull]
        private string GetComment()
        {
            return tokenizer.GetComment();
        }

        private string GetText()
        {
            return tokenizer.Text;
        }

        private void Initialize([NotNull] string query)
        {
            Debug.ArgumentNotNull(query, nameof(query));

            tokenizer = new QueryAnalyzerTokenizer(new QueryAnalyzerTokenBuilder(), query);
            builder = new QueryAnalyzerBuilder();

            SetField(this, "m_tokenizer", tokenizer);
            SetField(this, "m_builder", builder);

            Match();
        }

        private void SetField([NotNull] object obj, [NotNull] string name, [CanBeNull] object value)
        {
            Debug.ArgumentNotNull(obj, nameof(obj));
            Debug.ArgumentNotNull(name, nameof(name));

            var fieldInfo = typeof(QueryParser).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, value);
            }
        }
    }
}
