// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;

namespace Sitecore.Rocks.Server.QueryAnalyzers
{
    public class QueryAnalyzerQuery : Query
    {
        public QueryAnalyzerQuery(Opcode query) : base(query)
        {
        }

        public long Counter { get; set; }
    }
}
