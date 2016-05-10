// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers
{
    public interface IQueryAnalyzerKeyword
    {
        [CanBeNull]
        Opcode Parse([NotNull] Parser parser);
    }
}
