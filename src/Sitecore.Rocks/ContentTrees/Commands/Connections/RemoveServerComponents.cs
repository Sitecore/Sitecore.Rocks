// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.UpdateServerComponents.Pipelines.RemoveServerComponents;

namespace Sitecore.Rocks.ContentTrees.Commands.Connections
{
    [Command(Submenu = ConnectionsSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.RemoveServerComponents, typeof(ContentTreeContext))]
    public class RemoveServerComponents : CommandBase
    {
        public RemoveServerComponents()
        {
            Text = Resources.RemoveServerComponents_RemoveServerComponents_Remove_Server_Components___;
            Group = "ServerComponents";
            SortingValue = 4000;
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

            RemoveServerComponentsPipeline.Run().WithParameters(item.Site.DataService, item.Site.WebRootPath, item.Site);
        }
    }
}
