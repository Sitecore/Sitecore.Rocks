// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.SitecoreRocks.Extending
{
    [StartPageControl(StartPageSitecoreRocksPage.Name, 2000)]
    public class StartPageSitecoreRocksExtendingGroup : StartPageGroupBase
    {
        public const string Name = StartPageSitecoreRocksPage.Name + ".ExtendingSitecoreRocks";

        public StartPageSitecoreRocksExtendingGroup([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Extending Sitecore Rocks";
            Description = "Plugins and extensions can make Sitecore Rocks more powerful";
        }
    }
}
