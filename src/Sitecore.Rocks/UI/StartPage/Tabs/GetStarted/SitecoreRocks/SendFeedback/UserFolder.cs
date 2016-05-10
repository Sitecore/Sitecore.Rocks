// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Diagnostics;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.StartPage.Commands;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.SitecoreRocks.SendFeedback
{
    [Command, StartPageCommand("Open Sitecore Rocks user folder in Windows Explorer", StartPageSitecoreRocksSendFeedbackGroup.Name, 2000)]
    public class UserFolder : StartPageCommandBase
    {
        protected override void Execute()
        {
            Process.Start(@"explorer.exe", AppHost.User.UserFolder);
        }
    }
}
