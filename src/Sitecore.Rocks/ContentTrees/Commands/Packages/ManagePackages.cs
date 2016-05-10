// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.Packages.PackageManagers;
using Sitecore.Rocks.UI.StartPage;
using Sitecore.Rocks.UI.StartPage.Tabs.DevelopingTab.Deployment.Packages;

namespace Sitecore.Rocks.ContentTrees.Commands.Packages
{
    [Command, StartPageCommand("Manage and install packages", StartPagePackagesGroup.Name, 1000), Feature(FeatureNames.Packages)]
    public class ManagePackages : CommandBase, IStartPageCommand
    {
        public ManagePackages()
        {
            Text = Resources.ManagePackages_ManagePackages_Manage_Packages___;
            Group = "Site";
            SortingValue = 110;
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

            var site = context.SelectedItems.FirstOrDefault() as SiteTreeViewItem;
            if (site == null)
            {
                return false;
            }

            if (!site.Site.DataService.CanExecuteAsync("Packages.GetPackageFolder"))
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

            var siteTreeViewItem = context.SelectedItems.FirstOrDefault() as SiteTreeViewItem;
            if (siteTreeViewItem == null)
            {
                return;
            }

            var site = siteTreeViewItem.Site;

            Execute(site);
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

            var d = new PackageManagerDialog();

            d.Initialize(site);

            AppHost.Shell.ShowDialog(d);
        }
    }
}
