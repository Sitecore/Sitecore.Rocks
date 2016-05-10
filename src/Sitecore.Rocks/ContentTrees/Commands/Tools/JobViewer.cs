// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.StartPage;
using Sitecore.Rocks.UI.StartPage.Tabs.SystemTab.SystemInformation.Performance;

namespace Sitecore.Rocks.ContentTrees.Commands.Tools
{
    [Command(Submenu = ToolsSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.JobViewer, typeof(ContentTreeContext), ToolBar = ToolBars.SitecoreExplorer.Id, Group = ToolBars.SitecoreExplorer.Tools, Priority = 0x0730, Icon = "Resources/16x16/jobviewer.png"), StartPageCommand("View running background tasks in the Job Viewer", StartPagePerformanceGroup.Name, 2000), Feature(FeatureNames.CommonTools)]
    public class JobViewer : CommandBase, IStartPageCommand
    {
        public JobViewer()
        {
            Text = Resources.JobViewer_JobViewer_Job_Viewer;
            Group = "Applications";
            SortingValue = 8030;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ISiteSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (!context.Site.DataService.CanExecuteAsync("UI.JobViewer.GetJobs"))
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

            Execute(databaseUri.Site);
        }

        private void Execute([NotNull] Site site)
        {
            Debug.ArgumentNotNull(site, nameof(site));

            AppHost.Windows.OpenJobViewer(site);
        }
    }
}
