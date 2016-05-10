// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.WhereHandlers
{
    public interface IImportSource
    {
        [NotNull]
        int Execute([NotNull] Item item);

        [CanBeNull]
        void Parse([NotNull] Parser parser);
    }
}
