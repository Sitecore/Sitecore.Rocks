// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.SitecoreRocks
{
    [StartPageControl(StartPageGetStartedMainTab.Name, 3000)]
    public class StartPageSitecoreRocksPage : StartPageListBase
    {
        public const string Name = StartPageGetStartedMainTab.Name + ".SitecoreRocks";

        public StartPageSitecoreRocksPage([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Sitecore Rocks";
        }
    }
}
