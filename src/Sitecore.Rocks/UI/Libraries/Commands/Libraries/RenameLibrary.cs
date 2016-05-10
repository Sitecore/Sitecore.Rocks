// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.UI.Libraries.ContentTrees.VirtualItems;

namespace Sitecore.Rocks.UI.Libraries.Commands.Libraries
{
    [Command]
    public class RenameLibrary : CommandBase
    {
        public RenameLibrary()
        {
            Text = Resources.Rename_Rename_Rename;
            Group = "Library";
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

            var name = item.Library.Name;

            do
            {
                name = AppHost.Prompt("Enter the new name of the library:", "Quick Access", name);
                if (string.IsNullOrEmpty(name))
                {
                    return;
                }

                if (LibraryManager.Libraries.Any(w => w != item.Library && string.Compare(w.Name, name, StringComparison.InvariantCultureIgnoreCase) == 0))
                {
                    AppHost.MessageBox($"A library with the name '{name}' already exists.\n\nPlease choose another name.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    break;
                }
            }
            while (true);

            LibraryManager.Rename(item.Library, name);
        }
    }
}
