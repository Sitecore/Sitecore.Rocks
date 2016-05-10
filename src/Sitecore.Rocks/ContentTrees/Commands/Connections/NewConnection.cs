// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.ContentTrees.Commands.Connections
{
    [Command]
    public class NewConnection : CommandBase
    {
        public NewConnection()
        {
            Text = Resources.NewConnection_NewConnection_New_Connection;
            Group = "Connection";
            SortingValue = 50;
            Icon = new Icon("Resources/16x16/server_add.png");
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }

            if (!context.SelectedItems.Any())
            {
                return true;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return false;
            }

            var item = context.SelectedItems.FirstOrDefault() as ConnectionFolderTreeViewItem;
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

            if (!context.SelectedItems.Any())
            {
                SiteManager.NewConnection();
                return;
            }

            var item = context.SelectedItems.FirstOrDefault() as ConnectionFolderTreeViewItem;
            if (item == null)
            {
                return;
            }

            SiteManager.NewConnection(item);
        }
    }
}
