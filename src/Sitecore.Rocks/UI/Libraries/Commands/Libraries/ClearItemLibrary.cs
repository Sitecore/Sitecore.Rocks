// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.UI.Libraries.ContentTrees.VirtualItems;
using Sitecore.Rocks.UI.Libraries.ItemLibraries;

namespace Sitecore.Rocks.UI.Libraries.Commands.Libraries
{
    [Command]
    public class ClearItemLibrary : CommandBase
    {
        public ClearItemLibrary()
        {
            Text = "Clear";
            Group = "Edit";
            SortingValue = 5000;
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

            var item = context.SelectedItems.FirstOrDefault() as LibraryTreeViewItem;
            if (item == null)
            {
                return false;
            }

            return item.Library is ItemLibrary;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return;
            }

            var item = context.SelectedItems.FirstOrDefault() as LibraryTreeViewItem;
            if (item == null)
            {
                return;
            }

            var itemFolder = item.Library as ItemLibrary;
            if (itemFolder == null)
            {
                return;
            }

            if (AppHost.MessageBox(string.Format("Are you sure you want to clear '{0}'?", itemFolder.Name), "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            itemFolder.Items.Clear();
        }
    }
}
