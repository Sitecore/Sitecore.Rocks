// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Commands.Tools;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.Management;
using Sitecore.Rocks.UI.Management.ManagementItems.Validations;
using Sitecore.Rocks.UI.StartPage.Commands;

namespace Sitecore.Rocks.UI.StartPage.Tabs.SystemTab.SystemInformation.SystemHealth
{
    [Command(Submenu = ToolsSubmenu.Name), StartPageCommand("Run a validation assessment to find system issues", StartPageSystemHealthGroup.Name, 3000)]
    public class RunValidation : StartPageCommandBase
    {
        public RunValidation()
        {
            Text = "Validations";
            Group = "Manage";
            SortingValue = 8190;
        }

        public override bool CanExecute(StartPageContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            return this.HasDatabaseUri(context);
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

        public override void Execute(object parameter)
        {
            var context = parameter as ISiteSelectionContext;
            if (context == null)
            {
                return;
            }

            Execute(context.Site);
        }

        public override void Execute(StartPageContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

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

            AppHost.Windows.Factory.OpenManagementViewer(site.Name, new SiteManagementContext(site), ValidationViewer.ItemName);
        }
    }
}
