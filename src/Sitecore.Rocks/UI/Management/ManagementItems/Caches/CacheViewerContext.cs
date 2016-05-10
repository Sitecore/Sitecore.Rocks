// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Caches
{
    public class CacheViewerContext : ICommandContext
    {
        public CacheViewerContext([NotNull] CacheViewer cacheViewer)
        {
            Assert.ArgumentNotNull(cacheViewer, nameof(cacheViewer));

            CacheViewer = cacheViewer;
        }

        public CacheViewer CacheViewer { get; private set; }
    }
}
