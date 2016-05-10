// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.StartPage.Commands;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.Welcome.LearnAboutSitecore
{
    [Command, StartPageCommand("Read: Sitecore Partner Network (SPN)", StartPageLearnAboutSitecoreGroup.Name, 3000)]
    public class OpenSpnWebSite : StartPageCommandBase
    {
        protected override void Execute()
        {
            AppHost.Browsers.Navigate(@"http://spn.sitecore.net");
        }
    }
}
