// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Sites.Connections;

namespace Sitecore.Rocks.ContentTrees.Commands.ConnectionFolders
{
    [Command]
    public class NewConnectionFolder : CommandBase
    {
        public NewConnectionFolder()
        {
            Text = "New Connection Folder...";
            Group = "Connection";
            SortingValue = 100;
            Icon = new Icon("Resources/16x16/newfolder.png");
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

            var folderName = AppHost.Prompt("Enter the name of the folder:", "Connections", string.Empty);
            if (string.IsNullOrEmpty(folderName))
            {
                return;
            }

            if (!context.SelectedItems.Any())
            {
                folderName = Path.Combine(ConnectionManager.GetConnectionFolder(), folderName);
                Directory.CreateDirectory(folderName);

                var item = new ConnectionFolderTreeViewItem(folderName);
                context.ContentTree.TreeView.Items.Add(item);

                return;
            }

            var parentItem = context.SelectedItems.FirstOrDefault() as ConnectionFolderTreeViewItem;
            if (parentItem == null)
            {
                return;
            }

            folderName = Path.Combine(parentItem.Folder, folderName);
            Directory.CreateDirectory(folderName);
            parentItem.Refresh();
        }
    }
}
