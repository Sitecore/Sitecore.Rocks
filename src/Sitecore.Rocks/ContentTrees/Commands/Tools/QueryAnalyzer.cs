// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.StartPage;
using Sitecore.Rocks.UI.StartPage.Tabs.DevelopingTab.Querying.SitecoreQuery;

namespace Sitecore.Rocks.ContentTrees.Commands.Tools
{
    [Command(Submenu = ToolsSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.QueryAnalyzer, typeof(ContentTreeContext), ToolBar = ToolBars.SitecoreExplorer.Id, Group = ToolBars.SitecoreExplorer.Tools, Priority = 0x0730, Icon = "Resources/16x16/queryanalyzer.png"), StartPageCommand("Use the Query Analyzer to perform select, insert, update and delete operations", StartPageSitecoreQueryGroup.Name, 1000), Feature(FeatureNames.CommonTools)]
    public class QueryAnalyzer : CommandBase, IStartPageCommand
    {
        public QueryAnalyzer()
        {
            Text = Resources.QueryAnalyzer_QueryAnalyzer_Query_Analyzer;
            Group = "Applications";
            SortingValue = 8040;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IDatabaseSelectionContext;
            if (context == null)
            {
                return false;
            }

            var databaseUri = context.DatabaseUri;
            if (databaseUri == DatabaseUri.Empty)
            {
                return false;
            }

            if (!databaseUri.Site.DataService.CanExecuteAsync("QueryAnalyzer.Run"))
            {
                return false;
            }

            return databaseUri != DatabaseUri.Empty;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IDatabaseSelectionContext;
            if (context == null)
            {
                return;
            }

            AppHost.Windows.Factory.OpenQueryAnalyzer(context.DatabaseUri);
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
                AppHost.Windows.Factory.OpenQueryAnalyzer(databaseUri);
            }
        }
    }
}
