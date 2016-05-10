// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.SitecoreRocks.Plugins
{
    [StartPageControl(StartPageSitecoreRocksPage.Name, 2100)]
    public class StartPageSitecoreRocksPluginsGroup : StartPageGroupBase
    {
        public const string Name = StartPageSitecoreRocksPage.Name + ".Plugins";

        public StartPageSitecoreRocksPluginsGroup([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Plugins";
            Description = "Developing new plugins and extensions for Sitecore Rocks.";
        }
    }
}
