// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    public interface IFunction
    {
        [CanBeNull]
        object Invoke([NotNull] FunctionArgs args);
    }
}
