// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.StartPage.Commands;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.SitecoreRocks.Plugins
{
    [Command, StartPageCommand("Read: Creating Visual Studio project for developing a plugin", StartPageSitecoreRocksPluginsGroup.Name, 1000)]
    public class VisualStudioProject : StartPageCommandBase
    {
        protected override void Execute()
        {
            AppHost.Browsers.Navigate(@"https://github.com/Sitecore/Sitecore.Rocks/blob/master/docs/Plugins/CreatingVisualStudioProjects.md");
        }
    }
}
