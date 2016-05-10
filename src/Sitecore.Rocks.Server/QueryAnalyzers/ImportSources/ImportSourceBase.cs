// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;
using Sitecore.Rocks.Server.QueryAnalyzers.WhereHandlers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.ImportSources
{
    public abstract class ImportSourceBase : IImportSource
    {
        protected string Block { get; set; }

        protected string FileName { get; set; }

        [NotNull]
        public abstract int Execute(Item item);

        public abstract void Parse(Parser parser);

        [NotNull]
        protected string GetSource()
        {
            if (!string.IsNullOrEmpty(Block))
            {
                return Block;
            }

            return FileUtil.ReadUTF8File(FileName);
        }

        protected void ParseSource([NotNull] Parser parser)
        {
            Debug.ArgumentNotNull(parser, nameof(parser));

            if (parser.Token.Type == QueryAnalyzerTokenType.File)
            {
                parser.Match(QueryAnalyzerTokenType.File, "\"file\" expected");

                FileName = parser.Token.Value;

                parser.Match(TokenType.Literal, "File name expected");
            }
            else if (parser.Token.Type == TokenType.Literal)
            {
                var i = parser.Token.Index + parser.Token.Whitespace.Length;
                var text = parser.DoGetText();
                var quote = text[i].ToString();

                var s = parser.Token.Value;
                parser.Match();

                while (parser.Token.Type == TokenType.Literal)
                {
                    s += quote + quote;
                    s += parser.Token.Value;
                    parser.Match();
                }

                Block = s;
            }
        }
    }
}
