// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Management;

namespace Sitecore.Rocks.ContentTrees.Commands.Tools
{
    [Command, CommandId(CommandIds.SitecoreExplorer.ManageSite, typeof(ContentTreeContext), Text = "Manage Site"), Feature(FeatureNames.Management)]
    public class ManageSite : CommandBase
    {
        public ManageSite()
        {
            Text = Resources.ManageSite_ManageSite_Manage;
            Group = "Site";
            SortingValue = 100;
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

            var siteTreeViewItem = context.SelectedItems.FirstOrDefault() as SiteTreeViewItem;
            if (siteTreeViewItem == null)
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

            var item = context.SelectedItems.FirstOrDefault() as SiteTreeViewItem;
            if (item == null)
            {
                return;
            }

            AppHost.Windows.Factory.OpenManagementViewer(item.Site.Name, new SiteManagementContext(item.Site), string.Empty);
        }
    }
}
