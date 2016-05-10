// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;

namespace Sitecore.Rocks.ContentTrees.Commands.Sorting.Files
{
    [Command(Submenu = "FileSorting")]
    public class FileSortNone : CommandBase
    {
        public FileSortNone()
        {
            Text = "None";
            Group = "NoFileSorting";
            SortingValue = 9000;
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

            var item = context.SelectedItems.FirstOrDefault() as FileTreeViewItem;
            if (item == null)
            {
                return false;
            }

            if (!item.FileUri.IsFolder)
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

            var item = context.SelectedItems.FirstOrDefault() as FileTreeViewItem;
            if (item == null)
            {
                return;
            }

            FileSortManager.SetFileSort(item.FileUri, string.Empty);

            item.Refresh();
        }
    }
}
