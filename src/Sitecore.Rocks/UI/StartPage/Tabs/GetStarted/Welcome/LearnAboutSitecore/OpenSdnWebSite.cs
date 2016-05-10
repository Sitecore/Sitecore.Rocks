// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.StartPage.Commands;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.Welcome.LearnAboutSitecore
{
    [Command, StartPageCommand("Read: Sitecore Developer Network (SDN)", StartPageLearnAboutSitecoreGroup.Name, 3000)]
    public class OpenSdnWebSite : StartPageCommandBase
    {
        protected override void Execute()
        {
            AppHost.Browsers.Navigate(@"http://sdn.sitecore.net");
        }
    }
}
