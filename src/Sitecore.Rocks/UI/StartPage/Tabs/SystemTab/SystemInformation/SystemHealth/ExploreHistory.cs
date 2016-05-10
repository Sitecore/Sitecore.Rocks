// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Commands.Tools;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.Management;
using Sitecore.Rocks.UI.Management.ManagementItems.History;
using Sitecore.Rocks.UI.StartPage.Commands;

namespace Sitecore.Rocks.UI.StartPage.Tabs.SystemTab.SystemInformation.SystemHealth
{
    [Command(Submenu = ToolsSubmenu.Name), StartPageCommand("View audit information in the History table", StartPageSystemHealthGroup.Name, 2000), Feature(FeatureNames.Management)]
    public class ExploreHistory : StartPageCommandBase
    {
        public ExploreHistory()
        {
            Text = "History Table Viewer";
            Group = "Manage";
            SortingValue = 8120;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ISiteSelectionContext;
            if (context == null)
            {
                return false;
            }

            return context.Site != Site.Empty;
        }

        public override bool CanExecute(StartPageContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            return this.HasDatabaseUri(context);
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IDatabaseSelectionContext;
            if (context == null)
            {
                return;
            }

            Execute(context.DatabaseUri.Site, context.DatabaseUri);
        }

        public override void Execute(StartPageContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            var databaseUri = this.GetDatabaseUri(context);
            if (databaseUri == DatabaseUri.Empty)
            {
                return;
            }

            Execute(databaseUri.Site, databaseUri);
        }

        private void Execute([NotNull] Site site, [NotNull] DatabaseUri databaseUri)
        {
            Debug.ArgumentNotNull(site, nameof(site));
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));

            AppHost.Windows.Factory.OpenManagementViewer(site.Name, new DatabaseManagementContext(databaseUri), HistoryViewer.ItemName);
        }
    }
}
