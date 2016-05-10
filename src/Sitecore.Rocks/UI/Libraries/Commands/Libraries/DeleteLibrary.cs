// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Libraries.ContentTrees.VirtualItems;

namespace Sitecore.Rocks.UI.Libraries.Commands.Libraries
{
    [Command]
    public class DeleteLibrary : CommandBase
    {
        public DeleteLibrary()
        {
            Text = Resources.Delete_Delete_Delete;
            Group = "Edit";
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

            var item = context.SelectedItems.FirstOrDefault() as LibraryTreeViewItem;
            return item != null;
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

            if (AppHost.MessageBox(string.Format("Are you sure you want to delete '{0}'?", item.Library.Name), "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            LibraryManager.Delete(item.Library);
        }
    }
}
