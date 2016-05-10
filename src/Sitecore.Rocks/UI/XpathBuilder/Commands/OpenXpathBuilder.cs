// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.ContentTrees.Commands.Tools;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.StartPage;
using Sitecore.Rocks.UI.StartPage.Tabs.DevelopingTab.Querying.SitecoreQuery;

namespace Sitecore.Rocks.UI.XpathBuilder.Commands
{
    [Command(Submenu = ToolsSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.XPathBuilder, typeof(ContentTreeContext), ToolBar = ToolBars.SitecoreExplorer.Id, Group = ToolBars.SitecoreExplorer.Tools, Priority = 0x0730, Icon = "Resources/16x16/xpathbuilder.png"), StartPageCommand("Build a XPath expression using the XPath Builder", StartPageSitecoreQueryGroup.Name, 2000), Feature(FeatureNames.AdvancedTools)]
    public class OpenXpathBuilder : CommandBase, IStartPageCommand
    {
        public OpenXpathBuilder()
        {
            Text = Resources.XpathBuilderCommand_XpathBuilderCommand_XPath_Builder;
            Group = "Applications";
            SortingValue = 8050;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IDatabaseSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (context.DatabaseUri == DatabaseUri.Empty)
            {
                return false;
            }

            if (!context.DatabaseUri.Site.DataService.CanExecuteAsync("UI.XpathBuilder.Evaluate"))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IDatabaseSelectionContext;
            if (context == null)
            {
                return;
            }

            AppHost.Windows.Factory.OpenXpathBuilder(context.DatabaseUri);
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
            if (databaseUri == DatabaseUri.Empty)
            {
                return;
            }

            AppHost.Windows.Factory.OpenXpathBuilder(databaseUri);
        }
    }
}
