// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Repositories;
using Sitecore.Rocks.UI.StartPage.Commands;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.SitecoreRocks.Extending
{
    [Command, StartPageCommand("Import Start Page", StartPageSitecoreRocksExtendingGroup.Name, 3000)]
    public class ImportStartPage : StartPageCommandBase
    {
        public override void Execute(StartPageContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            var repository = RepositoryManager.GetRepository(RepositoryManager.StartPages);

            var fileName = repository.AddFile("Import Start Page");
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            StartPageManager.Reload();

            context.StartPageViewer.Tabs.RenderStartPage();
        }
    }
}
