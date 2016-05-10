// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage
{
    public class StartPageContext : ICommandContext
    {
        public StartPageContext([NotNull] StartPageViewer startPageViewer)
        {
            Assert.ArgumentNotNull(startPageViewer, nameof(startPageViewer));

            StartPageViewer = startPageViewer;
        }

        [NotNull]
        public StartPageViewer StartPageViewer { get; private set; }
    }
}
