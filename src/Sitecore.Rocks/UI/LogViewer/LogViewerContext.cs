// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.LogViewer
{
    public class LogViewerContext : ICommandContext, ISiteContext
    {
        public LogViewerContext([NotNull] LogViewer logViewer)
        {
            Assert.ArgumentNotNull(logViewer, nameof(logViewer));

            LogViewer = logViewer;
        }

        public LogViewer LogViewer { get; set; }

        public Site Site
        {
            get { return LogViewer.Site; }
        }

        public void SetSite(Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            LogViewer.SetSite(site);
        }
    }
}
