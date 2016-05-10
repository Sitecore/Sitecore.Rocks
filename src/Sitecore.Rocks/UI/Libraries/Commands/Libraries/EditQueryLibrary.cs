// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Libraries.ContentTrees.VirtualItems;
using Sitecore.Rocks.UI.Libraries.Dialogs;
using Sitecore.Rocks.UI.Libraries.QueryLibraries;

namespace Sitecore.Rocks.UI.Libraries.Commands.Libraries
{
    [Command]
    public class EditQueryLibrary : CommandBase
    {
        public EditQueryLibrary()
        {
            Text = "Edit Query Library...";
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

            return item.Library is QueryLibrary;
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

            var library = item.Library as QueryLibrary;
            if (library == null)
            {
                return;
            }

            var dialog = new QueryDialog(library.Query, library.DatabaseUri, library.Name)
            {
                IsQueryNameVisible = false
            };

            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            library.Query = dialog.Query;
            library.DatabaseUri = dialog.DatabaseUri ?? DatabaseUri.Empty;
            library.Save();
            library.Refresh();
            library.IsExpanded = true;
        }
    }
}
