// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.UpdateServerComponents;

namespace Sitecore.Rocks.ContentTrees.Commands.Connections
{
    [Command(Submenu = ConnectionsSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.UpdateServerComponents, typeof(ContentTreeContext))]
    public class UpdateServerComponents : CommandBase
    {
        public UpdateServerComponents()
        {
            Text = Resources.UpdateServerComponents_UpdateServerComponents_Update_Server_Components___;
            Group = "ServerComponents";
            SortingValue = 3000;
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

            if (!siteTreeViewItem.Site.DataService.HasWebSite)
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

            var d = new UpdateServerComponentsDialog();

            d.Initialize(item.Site.DataService, item.Site.Name, item.Site.WebRootPath, item.Site, null);
            try
            {
                AppHost.Shell.ShowDialog(d);
            }
            catch (Exception ex)
            {
                AppHost.Output.LogException(ex);
            }
        }
    }
}
