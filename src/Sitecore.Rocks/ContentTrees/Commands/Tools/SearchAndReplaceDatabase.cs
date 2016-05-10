// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.StartPage;
using Sitecore.Rocks.UI.StartPage.Tabs.ContentTab.Items.ContentItems;

namespace Sitecore.Rocks.ContentTrees.Commands.Tools
{
    [Command(Submenu = ToolsSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.SearchAndReplace, typeof(ContentTreeContext)), StartPageCommand("Search and replace text in fields", StartPageSearchAndReplaceGroup.Name, 1000), Feature(FeatureNames.AdvancedTools)]
    public class SearchAndReplaceDatabase : CommandBase, IStartPageCommand
    {
        public SearchAndReplaceDatabase()
        {
            Text = Resources.SearchAndReplace;
            Group = "Applications";
            SortingValue = 8400;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return false;
            }

            var item = context.SelectedItems.FirstOrDefault() as DatabaseTreeViewItem;
            if (item == null)
            {
                return false;
            }

            if (!item.DatabaseUri.Site.DataService.CanExecuteAsync("QueryAnalyzer.Run"))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return;
            }

            var item = context.SelectedItems.FirstOrDefault() as DatabaseTreeViewItem;
            if (item == null)
            {
                return;
            }

            AppHost.Windows.OpenSearchAndReplace(item.DatabaseUri);
        }

        bool IStartPageCommand.CanExecute(StartPageContext context)
        {
            Debug.ArgumentNotNull(context, nameof(context));

            return this.HasDatabaseUri(context);
        }

        void IStartPageCommand.Execute(StartPageContext context)
        {
            Debug.ArgumentNotNull(context, nameof(context));

            var databaseUri = this.GetDatabaseUri(context);
            if (databaseUri != DatabaseUri.Empty)
            {
                AppHost.Windows.Factory.OpenSearchAndReplace(databaseUri);
            }
        }
    }
}
