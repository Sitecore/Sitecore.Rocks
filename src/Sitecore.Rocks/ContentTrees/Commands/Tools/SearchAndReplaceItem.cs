// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Tools
{
    [Command(Submenu = ToolsSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.SearchAndReplace, typeof(ContentTreeContext)), Feature(FeatureNames.AdvancedTools)]
    public class SearchAndReplaceItem : CommandBase
    {
        public SearchAndReplaceItem()
        {
            Text = Resources.SearchAndReplace;
            Group = "Applications";
            SortingValue = 8400;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (context.Items.Count() != 1)
            {
                return false;
            }

            var item = context.Items.FirstOrDefault();
            if (item == null)
            {
                return false;
            }

            if (!item.ItemUri.Site.DataService.CanExecuteAsync("QueryAnalyzer.Run"))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            if (context.Items.Count() != 1)
            {
                return;
            }

            var item = context.Items.FirstOrDefault();
            if (item == null)
            {
                return;
            }

            AppHost.Windows.OpenSearchAndReplace(item.ItemUri);
        }
    }
}
