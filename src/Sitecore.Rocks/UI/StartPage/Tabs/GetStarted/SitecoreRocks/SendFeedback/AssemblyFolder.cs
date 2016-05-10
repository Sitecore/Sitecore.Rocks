// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Diagnostics;
using System.IO;
using System.Reflection;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.StartPage.Commands;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.SitecoreRocks.SendFeedback
{
    [Command, StartPageCommand("Open Sitecore Rocks application folder in Windows Explorer", StartPageSitecoreRocksSendFeedbackGroup.Name, 3000)]
    public class AssemblyFolder : StartPageCommandBase
    {
        protected override void Execute()
        {
            Process.Start(@"explorer.exe", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        }
    }
}
