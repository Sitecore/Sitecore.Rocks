// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Management.ManagementItems.SiteViewers
{
    public class SiteViewerContext : ICommandContext
    {
        public SiteViewerContext(SiteViewer pipelines)
        {
            SiteViewer = pipelines;
        }

        public SiteViewer SiteViewer { get; private set; }
    }
}
