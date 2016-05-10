// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.StartPage.Commands;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.SitecoreRocks.Extending
{
    [Command, StartPageCommand("Create a Visual Studio project for a new Sitecore Rocks plugin", StartPageSitecoreRocksExtendingGroup.Name, 2900)]
    public class CreatePluginVisualStudioProject : StartPageCommandBase
    {
        public override void Execute(StartPageContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            AppHost.Projects.CreateVisualStudioProject("Sitecore.Rocks.Plugin.zip", string.Empty, "Sitecore.Rocks.Plugin");
        }
    }
}
