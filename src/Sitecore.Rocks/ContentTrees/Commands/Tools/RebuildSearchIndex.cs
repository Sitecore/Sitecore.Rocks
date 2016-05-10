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
using Sitecore.Rocks.UI.StartPage.Tabs.SystemTab.SystemInformation.SearchIndexes;

namespace Sitecore.Rocks.ContentTrees.Commands.Tools
{
    [Command(Submenu = TasksSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.RebuildSearchIndex, typeof(ContentTreeContext)), StartPageCommand("Rebuild the indexes", StartPageSearchIndexesGroup.Name, 1550)]
    public class RebuildSearchIndex : CommandBase, IStartPageCommand
    {
        public RebuildSearchIndex()
        {
            Text = Resources.RebuildSearchIndex_RebuildSearchIndex_Rebuild_Search_Index;
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

            if ((item.Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.RebuildSearchIndex) != DataServiceFeatureCapabilities.RebuildSearchIndex)
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
            if (databaseUri == DatabaseUri.Empty)
            {
                return;
            }

            Execute(databaseUri);
        }

        private void Execute([NotNull] DatabaseUri databaseUri)
        {
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));

            AppHost.Server.RebuildSearchIndex(databaseUri, AppHost.Server.HandleCompleted);

            AppHost.MessageBox(Resources.RebuildSearchIndex_Execute_Rebuild_Search_Index_started___, Resources.RebuildSearchIndex_Execute_Rebuild_Search_Index, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
