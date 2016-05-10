// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.StartPage.Commands;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.SitecoreRocks.LearnAboutSitecoreRocks
{
    [Command, StartPageCommand("Read: Official Sitecore Rocks web site", StartPageLearnAboutSitecoreRocksGroup.Name, 500)]
    public class OpenVsPlugins : StartPageCommandBase
    {
        protected override void Execute()
        {
            AppHost.Browsers.Navigate(@"http://vsplugins.sitecore.net");
        }
    }
}
