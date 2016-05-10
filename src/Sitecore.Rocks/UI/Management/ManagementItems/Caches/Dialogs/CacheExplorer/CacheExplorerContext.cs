// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Caches.Dialogs.CacheExplorer
{
    public class CacheExplorerContext : ICommandContext
    {
        public CacheExplorerContext([NotNull] CacheExplorerDialog cacheExplorer)
        {
            Assert.ArgumentNotNull(cacheExplorer, nameof(cacheExplorer));

            CacheExplorer = cacheExplorer;
        }

        public CacheExplorerDialog CacheExplorer { get; private set; }
    }
}
