// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Commands.Tools;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.UI.Management;
using Sitecore.Rocks.UI.Management.ManagementItems.LanguageViewers;
using Sitecore.Rocks.UI.StartPage.Commands;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.Configuring.SystemItems
{
    [Command(Submenu = ToolsSubmenu.Name), StartPageCommand("Explore and create new content languages", StartPageSystemItemsGroup.Name, 1000), Feature(FeatureNames.Management)]
    public class ExploreLanguages : StartPageCommandBase
    {
        public ExploreLanguages()
        {
            Text = "Languages";
            Group = "Manage";
            SortingValue = 8190;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IDatabaseSelectionContext;
            if (context == null)
            {
                return false;
            }

            return context.DatabaseUri != DatabaseUri.Empty;
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

            Execute(context.DatabaseUri);
        }

        public override void Execute(StartPageContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

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

            AppHost.Windows.Factory.OpenManagementViewer(databaseUri.DatabaseName + @" - " + databaseUri.Site.Name, new DatabaseManagementContext(databaseUri), LanguageViewer.ItemName);
        }
    }
}
