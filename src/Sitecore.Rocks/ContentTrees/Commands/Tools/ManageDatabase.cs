// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Management;

namespace Sitecore.Rocks.ContentTrees.Commands.Tools
{
    [Command, CommandId(CommandIds.SitecoreExplorer.ManageDatabase, typeof(ContentTreeContext), Text = "Manage Database"), Feature(FeatureNames.Management)]
    public class ManageDatabase : CommandBase
    {
        public ManageDatabase()
        {
            Text = Resources.ManageSite_ManageSite_Manage;
            Group = "Management";
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

            var item = context.SelectedItems.FirstOrDefault() as DatabaseTreeViewItem;
            if (item == null)
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

            AppHost.Windows.OpenManagementViewer(item.DatabaseUri.DatabaseName + " - " + item.Site.Name, new DatabaseManagementContext(item.DatabaseUri), string.Empty);
        }
    }
}
