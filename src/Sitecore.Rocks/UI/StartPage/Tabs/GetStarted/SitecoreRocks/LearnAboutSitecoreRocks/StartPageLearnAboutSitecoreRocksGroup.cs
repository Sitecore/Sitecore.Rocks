// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.SitecoreRocks.LearnAboutSitecoreRocks
{
    [StartPageControl(StartPageSitecoreRocksPage.Name, 1000)]
    public class StartPageLearnAboutSitecoreRocksGroup : StartPageGroupBase
    {
        public const string Name = StartPageSitecoreRocksPage.Name + ".AboutSitecoreRocks";

        public StartPageLearnAboutSitecoreRocksGroup([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Learn About Sitecore Rocks";
            Description = "Learn how to use Sitecore Rocks and get information about the current release.";
        }
    }
}
