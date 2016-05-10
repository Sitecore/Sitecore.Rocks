// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Management.ManagementItems.TempFiles
{
    public class TempFileViewerContext : ICommandContext
    {
        public TempFileViewerContext([NotNull] TempFileViewer tempFileViewer)
        {
            Assert.ArgumentNotNull(tempFileViewer, nameof(tempFileViewer));

            TempFileViewer = tempFileViewer;
        }

        public TempFileViewer TempFileViewer { get; private set; }
    }
}
