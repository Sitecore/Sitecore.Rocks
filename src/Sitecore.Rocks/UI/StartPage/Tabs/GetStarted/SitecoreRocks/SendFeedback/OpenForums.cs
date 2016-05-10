// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.StartPage.Commands;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.SitecoreRocks.SendFeedback
{
    [Command, StartPageCommand("Report a bug or send feedback in the Sitecore Rocks forums", StartPageSitecoreRocksSendFeedbackGroup.Name, 1000)]
    public class OpenForums : StartPageCommandBase
    {
        protected override void Execute()
        {
            AppHost.Browsers.Navigate(@"http://sdn.sitecore.net/Forum/ShowForum.aspx?ForumID=36");
        }
    }
}
