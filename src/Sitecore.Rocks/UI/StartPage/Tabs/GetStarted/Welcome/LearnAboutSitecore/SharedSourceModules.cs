// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.StartPage.Commands;
using Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.Configuring.Extending;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.Welcome.LearnAboutSitecore
{
    [Command, StartPageCommand("Read: Shared Source Modules (trac.sitecore.net)", StartPageExtendingGroup.Name, 1000)]
    public class SharedSourceModules : StartPageCommandBase
    {
        protected override void Execute()
        {
            AppHost.Browsers.Navigate(@"http://trac.sitecore.net/Index");
        }
    }
}
