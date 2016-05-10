// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Sites.Connections;

namespace Sitecore.Rocks.ContentTrees.Commands.ConnectionFolders
{
    [Command]
    public class DeleteConnectionFolder : CommandBase
    {
        public DeleteConnectionFolder()
        {
            Text = "Remove...";
            Group = "Connection";
            SortingValue = 3000;
            Icon = new Icon("Resources/16x16/delete.png");
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

            var item = context.SelectedItems.FirstOrDefault() as ConnectionFolderTreeViewItem;
            if (item == null)
            {
                return false;
            }

            if (item.Folder == ConnectionManager.GetConnectionFolder())
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

            var selectedItems = new List<ICanDelete>();

            foreach (var selectedItem in context.SelectedItems)
            {
                var item = selectedItem as ICanDelete;
                if (item == null)
                {
                    return;
                }

                selectedItems.Add(item);
            }

            if (selectedItems.Count == 1)
            {
                var item = selectedItems[0];

                if (AppHost.MessageBox(string.Format(Resources.Delete_Execute_Are_you_sure_you_want_to_delete___0___, item.Text), Resources.Confirmation, MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) != MessageBoxResult.OK)
                {
                    return;
                }
            }
            else
            {
                if (AppHost.MessageBox(string.Format(Resources.Delete_Execute_Are_you_sure_you_want_to_delete_these__0__items_, selectedItems.Count), Resources.Confirmation, MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) != MessageBoxResult.OK)
                {
                    return;
                }
            }

            foreach (var item in selectedItems)
            {
                item.Delete(false);

                var treeViewItem = item as TreeViewItem;
                if (treeViewItem == null)
                {
                    continue;
                }

                var parent = treeViewItem.Parent as TreeViewItem;
                if (parent != null)
                {
                    parent.Items.Remove(treeViewItem);
                }
                else
                {
                    context.ContentTree.TreeView.Items.Remove(treeViewItem);
                }
            }
        }
    }
}
