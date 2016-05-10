// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using Sitecore.Rocks.Server.QueryAnalyzers;

namespace Sitecore.Rocks.Server.Requests.QueryAnalyzer
{
    public class GetKeywords
    {
        [NotNull]
        public string Execute()
        {
            var writer = new StringWriter();

            foreach (var keyword in QueryAnalyzerManager.Keywords)
            {
                writer.WriteLine(keyword.Attribute.Keyword);
            }

            foreach (var reservedWord in QueryAnalyzerManager.ReservedWords)
            {
                writer.WriteLine(reservedWord.Attribute.Word);
            }

            foreach (var importSource in QueryAnalyzerManager.ImportSources)
            {
                writer.WriteLine(importSource.Attribute.Word);
            }

            foreach (var whereHandler in QueryAnalyzerManager.WhereHandlers)
            {
                writer.WriteLine(whereHandler.Attribute.Word);
            }

            return writer.ToString();
        }
    }
}
