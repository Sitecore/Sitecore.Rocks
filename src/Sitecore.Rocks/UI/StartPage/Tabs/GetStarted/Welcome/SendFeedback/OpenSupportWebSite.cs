// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.StartPage.Commands;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.Welcome.SendFeedback
{
    [Command, StartPageCommand("Report a bug on the Sitecore Support Portal web site", StartPageSendFeedbackGroup.Name, 1000)]
    public class OpenSupportWebSite : StartPageCommandBase
    {
        protected override void Execute()
        {
            AppHost.Browsers.Navigate(@"http://support.sitecore.net");
        }
    }
}
