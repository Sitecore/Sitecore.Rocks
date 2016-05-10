// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Indexes
{
    public class IndexViewerContext : ICommandContext
    {
        public const string FieldList = "FieldList";

        public const string IndexList = "IndexList";

        public IndexViewerContext([NotNull] IndexViewer indexViewer)
        {
            Assert.ArgumentNotNull(indexViewer, nameof(indexViewer));

            IndexViewer = indexViewer;
        }

        public string ClickTarget { get; set; }

        public IndexViewer IndexViewer { get; private set; }
    }
}
