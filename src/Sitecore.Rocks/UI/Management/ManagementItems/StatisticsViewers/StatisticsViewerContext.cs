// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Management.ManagementItems.StatisticsViewers
{
    public class StatisticsViewerContext : ICommandContext
    {
        public StatisticsViewerContext([NotNull] StatisticsViewer statisticsViewer)
        {
            Assert.ArgumentNotNull(statisticsViewer, nameof(statisticsViewer));

            StatisticsViewer = statisticsViewer;
        }

        public StatisticsViewer StatisticsViewer { get; private set; }
    }
}
