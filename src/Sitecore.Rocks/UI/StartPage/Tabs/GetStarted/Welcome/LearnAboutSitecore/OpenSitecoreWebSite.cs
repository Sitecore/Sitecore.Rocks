// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.StartPage.Commands;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.Welcome.LearnAboutSitecore
{
    [Command, StartPageCommand("Read: Official Sitecore Web Site", StartPageLearnAboutSitecoreGroup.Name, 1000)]
    public class OpenSitecoreWebSite : StartPageCommandBase
    {
        protected override void Execute()
        {
            AppHost.Browsers.Navigate(@"http://www.sitecore.net");
        }
    }
}
