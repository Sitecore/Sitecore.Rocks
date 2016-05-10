// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("import", QueryAnalyzerTokenType.Import, ShortHelp = "Import items from an external source", LongHelp = "Imports items from an external like a file or an Xml string.", Syntax = "'import' [SourceType] [Source]", Example = "import csv file \"c:\\import.csv\"\n" + "import csv \"<csv data>\" "), ReservedWord("file", QueryAnalyzerTokenType.File)]
    public class ImportKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));

            parser.Match(QueryAnalyzerTokenType.Import, "\"import\" expected");

            var importSource = parser.GetImportSource();

            return new Import(importSource);
        }
    }
}
