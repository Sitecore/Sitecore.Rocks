// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Commands.Tools;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.ContentTrees.Commands.Searching
{
    [Command(Submenu = ToolsSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.Search, typeof(ContentTreeContext), ToolBar = ToolBars.SitecoreExplorer.Id, Group = ToolBars.SitecoreExplorer.Tools, Priority = 0x0700, Icon = "Resources/16x16/search.png"), Feature(FeatureNames.CommonTools)]
    public class Search : CommandBase
    {
        public Search()
        {
            Text = Resources.Search_Search_Search;
            Group = "Applications";
            SortingValue = 8000;
            Icon = new Icon("Resources/16x16/search.png");
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ISiteSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (!context.Site.DataService.CanExecuteAsync("Search.Search"))
            {
                return false;
            }

            return context.Site != Site.Empty;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ISiteSelectionContext;
            if (context != null)
            {
                AppHost.Windows.Factory.OpenSearchViewer(context.Site);
            }
        }
    }
}
