// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Reflection;
using System.Text;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Parsers
{
    public class QueryAnalyzerTokenizer : Tokenizer
    {
        private readonly FieldInfo indexFieldInfo;

        public QueryAnalyzerTokenizer([NotNull] TokenBuilder builder, [NotNull] string text) : base(builder, text)
        {
            Assert.ArgumentNotNull(builder, nameof(builder));
            Assert.ArgumentNotNull(text, nameof(text));

            Text = text;
            Builder = builder;
            indexFieldInfo = typeof(Tokenizer).GetField("m_index", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public TokenBuilder Builder { get; set; }

        public int Index
        {
            get { return (int)indexFieldInfo.GetValue(this); }

            set { indexFieldInfo.SetValue(this, value); }
        }

        public string Text { get; set; }

        [NotNull]
        public string GetBlock()
        {
            var index = Index;

            while (index < Text.Length && char.IsWhiteSpace(Text[index]))
            {
                index++;
            }

            if (index >= Text.Length)
            {
                return string.Empty;
            }

            var quote = Text[index];
            if (quote != '"' && quote != '\'')
            {
                throw new Exception("Quote expected");
            }

            var sb = new StringBuilder();

            while (index < Text.Length)
            {
                var c = Text[index];
                if (c != quote)
                {
                    sb.Append(c);
                    index++;
                    continue;
                }

                if (index == Text.Length - 1)
                {
                    Index = index + 1;
                    return sb.ToString();
                }

                var nextChar = Text[index + 1];
                if (nextChar == quote)
                {
                    sb.Append(quote);
                    index += 2;
                    continue;
                }

                Index = index + 1;
                return sb.ToString();
            }

            throw new Exception("Unterminated literal");
        }

        [NotNull]
        public string GetComment()
        {
            var index = Index;
            var chars = Text.ToCharArray();

            var sb = new StringBuilder();

            while (index < chars.Length && chars[index] != '\n' && chars[index] != '\r')
            {
                sb.Append(chars[index]);
                index++;
            }

            Index = index + 1;

            return sb.ToString();
        }

        public override Token NextToken()
        {
            try
            {
                return base.NextToken();
            }
            catch (QueryException ex)
            {
                if (ex.Message == "Unexpected character ';'")
                {
                    Builder.Identifier(";");
                    Index++;
                    return Builder.Token;
                }

                throw;
            }
        }
    }
}
