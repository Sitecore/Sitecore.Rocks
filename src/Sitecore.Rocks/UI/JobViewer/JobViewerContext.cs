// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.JobViewer
{
    public class JobViewerContext : ICommandContext, ISiteContext
    {
        public JobViewerContext([NotNull] JobViewer logViewer)
        {
            Assert.ArgumentNotNull(logViewer, nameof(logViewer));

            JobViewer = logViewer;
        }

        public JobViewer JobViewer { get; set; }

        Site ISiteContext.Site
        {
            get { return JobViewer.Site; }
        }

        void ISiteContext.SetSite(Site site)
        {
            Debug.ArgumentNotNull(site, nameof(site));

            JobViewer.SetSite(site);
        }
    }
}
