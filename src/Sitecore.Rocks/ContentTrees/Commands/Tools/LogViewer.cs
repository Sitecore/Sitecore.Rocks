// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.StartPage;
using Sitecore.Rocks.UI.StartPage.Tabs.SystemTab.SystemInformation.SystemHealth;

namespace Sitecore.Rocks.ContentTrees.Commands.Tools
{
    [Command(Submenu = ToolsSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.LogViewer, typeof(ContentTreeContext), ToolBar = ToolBars.SitecoreExplorer.Id, Group = ToolBars.SitecoreExplorer.Tools, Priority = 0x0720, Icon = "Resources/16x16/logviewer.png"), StartPageCommand("View log information in the Log Viewer", StartPageSystemHealthGroup.Name, 1000), Feature(FeatureNames.CommonTools)]
    public class LogViewer : CommandBase, IStartPageCommand
    {
        public LogViewer()
        {
            Text = Resources.LogViewer_LogViewer_Log_Viewer;
            Group = "Applications";
            SortingValue = 8020;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ISiteSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (!context.Site.DataService.CanExecuteAsync("UI.LogViewer.GetLog"))
            {
                return false;
            }

            return context.Site != Site.Empty;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ISiteSelectionContext;
            if (context == null)
            {
                return;
            }

            Execute(context.Site);
        }

        public void Execute(StartPageContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            var databaseUri = this.GetDatabaseUri(context);
            if (databaseUri == DatabaseUri.Empty)
            {
                return;
            }

            Execute(databaseUri.Site);
        }

        bool IStartPageCommand.CanExecute(StartPageContext context)
        {
            Debug.ArgumentNotNull(context, nameof(context));

            return this.HasDatabaseUri(context);
        }

        private void Execute([NotNull] Site site)
        {
            Debug.ArgumentNotNull(site, nameof(site));

            AppHost.Windows.Factory.OpenLogViewer(site);
        }
    }
}
