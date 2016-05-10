// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Commands.Tasks;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.StartPage;
using Sitecore.Rocks.UI.StartPage.Tabs.SystemTab.SystemInformation.SystemHealth;

namespace Sitecore.Rocks.ContentTrees.Commands.Tools
{
    [Command(Submenu = TasksSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.RebuildLinkDatabase, typeof(ContentTreeContext)), StartPageCommand("Rebuild the Link Database", StartPageSystemHealthGroup.Name, 1560)]
    public class RebuildLinkDatabase : CommandBase, IStartPageCommand
    {
        public RebuildLinkDatabase()
        {
            Text = Resources.RebuildLinkDatabase_RebuildLinkDatabase_Rebuild_Link_Database;
            Group = "Rebuild";
            SortingValue = 1000;
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

            if ((item.Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.RebuildLinkDatabase) != DataServiceFeatureCapabilities.RebuildLinkDatabase)
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

            var databaseUri = item.DatabaseUri;
            Execute(databaseUri);
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
                Execute(databaseUri);
            }
        }

        private void Execute([NotNull] DatabaseUri databaseUri)
        {
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));

            AppHost.Server.RebuildLinkDatabase(databaseUri, AppHost.Server.HandleCompleted);

            AppHost.MessageBox(Resources.RebuildLinkDatabase_Execute_Rebuild_Link_Database_started___, Resources.RebuildLinkDatabase_Execute_Rebuild_Link_Database, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
