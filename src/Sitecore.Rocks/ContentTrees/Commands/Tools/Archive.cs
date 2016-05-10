// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.StartPage;
using Sitecore.Rocks.UI.StartPage.Tabs.SystemTab.Maintenance.Archives;

namespace Sitecore.Rocks.ContentTrees.Commands.Tools
{
    [Command(Submenu = ToolsSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.Archive, typeof(ContentTreeContext)), StartPageCommand("Open the Archive", StartPageArchivesGroup.Name, 2000), Feature(FeatureNames.AdvancedTools)]
    public class Archive : CommandBase, IStartPageCommand
    {
        public Archive()
        {
            Text = Resources.ArchivePane_ArchivePane_Archive;
            Group = "Applications";
            SortingValue = 8400;
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

            var databaseUri = context.DatabaseUri;

            if (!databaseUri.Site.DataService.CanExecuteAsync("Archives.GetArchivedItems"))
            {
                return false;
            }

            if ((databaseUri.Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Execute) != DataServiceFeatureCapabilities.Execute)
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

            Execute(context.DatabaseUri);
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

            var header = string.Format(@"Archive - {0} - {1}", databaseUri.Site.Name, databaseUri.DatabaseName);

            AppHost.Windows.Factory.OpenArchive(databaseUri, header);
        }
    }
}
