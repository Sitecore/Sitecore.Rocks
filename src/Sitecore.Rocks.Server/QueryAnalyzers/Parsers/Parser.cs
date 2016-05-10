// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.WhereHandlers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Parsers
{
    public class Parser
    {
        public Token Token
        {
            get { return GetToken(); }
        }

        internal Func<ElementBase> DoGetAttribute { get; set; }

        internal Func<string> DoGetBlock { get; set; }

        internal Func<string> DoGetComment { get; set; }

        internal Func<Opcode> DoGetExpression { get; set; }

        internal Func<Opcode> DoGetQueries { get; set; }

        internal Func<string> DoGetText { get; set; }

        internal Action DoMatch { get; set; }

        internal Action<int, string> DoMatchTokenType { get; set; }

        internal Func<Token> GetToken { get; set; }

        [CanBeNull]
        public Opcode GetAttribute()
        {
            return DoGetAttribute();
        }

        [CanBeNull]
        public string GetBlock()
        {
            return DoGetBlock();
        }

        [CanBeNull]
        public string GetComment()
        {
            return DoGetComment();
        }

        [CanBeNull]
        public Opcode GetExpression()
        {
            return DoGetExpression();
        }

        [CanBeNull]
        public Opcode GetFrom()
        {
            Match(QueryAnalyzerTokenType.From, "\"from\" expected");

            var opcode = QueryAnalyzerManager.GetFrom(this, Token.Type);
            if (opcode == null)
            {
                opcode = GetQueries();
            }

            return opcode;
        }

        [CanBeNull]
        public IImportSource GetImportSource()
        {
            return QueryAnalyzerManager.GetImportSource(this, Token.Type);
        }

        [CanBeNull]
        public Opcode GetQueries()
        {
            return DoGetQueries();
        }

        public void Match()
        {
            DoMatch();
        }

        public void Match(int tokenType, [NotNull] string message)
        {
            Assert.ArgumentNotNull(message, nameof(message));

            DoMatchTokenType(tokenType, message);
        }

        public void Raise([NotNull] string error)
        {
            Debug.ArgumentNotNull(error, nameof(error));

            throw new ParseException(error + " at position " + Token.Index + ".");
        }
    }
}
