// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Parsers
{
    public class QueryAnalyzerTokenBuilder : QueryTokenBuilder
    {
        public override void Identifier([NotNull] string value)
        {
            Assert.ArgumentNotNull(value, nameof(value));

            base.Identifier(value);
            if (Token.Type != TokenType.Identifier)
            {
                return;
            }

            var tokenType = QueryAnalyzerManager.GetTokenType(value);
            if (tokenType >= 0)
            {
                Token.Type = tokenType;
                Token.Value = value;
                return;
            }

            if (value == ";")
            {
                Token.Type = QueryAnalyzerTokenType.Semicolon;
                return;
            }

            Token.Type = TokenType.Identifier;
            Token.Value = value;
        }
    }
}
