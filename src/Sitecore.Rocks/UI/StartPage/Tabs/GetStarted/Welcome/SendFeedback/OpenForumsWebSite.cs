// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.StartPage.Commands;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.Welcome.SendFeedback
{
    [Command, StartPageCommand("Sitecore Forums on SDN", StartPageSendFeedbackGroup.Name, 2000)]
    public class OpenForumsWebSite : StartPageCommandBase
    {
        protected override void Execute()
        {
            AppHost.Browsers.Navigate(@"http://sdn.sitecore.net/Forum.aspx");
        }
    }
}
