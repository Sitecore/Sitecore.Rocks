// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.StartPage.Commands;
using Sitecore.Rocks.UI.StartPage.Tabs.ContentTab.Items.ContentItems;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.Welcome.Start
{
    [Command, StartPageCommand("Open the Sitecore Explorer window", StartPageStartGroup.Name, 2000), StartPageCommand("Explore the content items", StartPageContentItemsGroup.Name, 1000)]
    public class OpenSitecoreExplorer : StartPageCommandBase
    {
        public override void Execute(StartPageContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            AppHost.Windows.Factory.OpenContentTree();
        }
    }
}
