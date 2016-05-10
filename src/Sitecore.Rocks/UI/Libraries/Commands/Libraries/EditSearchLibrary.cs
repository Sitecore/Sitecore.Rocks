// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Libraries.ContentTrees.VirtualItems;
using Sitecore.Rocks.UI.Libraries.Dialogs;
using Sitecore.Rocks.UI.Libraries.SearchLibraries;

namespace Sitecore.Rocks.UI.Libraries.Commands.Libraries
{
    [Command]
    public class EditSearchLibrary : CommandBase
    {
        public EditSearchLibrary()
        {
            Text = "Edit Search Library...";
            Group = "Edit";
            SortingValue = 1000;
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

            return item.Library is SearchLibrary;
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

            var library = item.Library as SearchLibrary;
            if (library == null)
            {
                return;
            }

            var dialog = new SearchDialog(library.SearchText, library.DatabaseUri, library.Name)
            {
                IsSearchNameVisible = false
            };

            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            library.SearchText = dialog.SearchText;
            library.DatabaseUri = dialog.DatabaseUri ?? DatabaseUri.Empty;
            library.Save();
            library.Refresh();
            library.IsExpanded = true;
        }
    }
}
