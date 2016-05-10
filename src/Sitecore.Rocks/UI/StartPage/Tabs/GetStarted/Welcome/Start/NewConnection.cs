// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.StartPage.Commands;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.Welcome.Start
{
    [Command, StartPageCommand("Create a new Sitecore Rocks connection to a Sitecore web site", StartPageStartGroup.Name, 1000)]
    public class NewConnection : StartPageCommandBase
    {
        public override void Execute(StartPageContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            SiteManager.NewConnection();
        }
    }
}
