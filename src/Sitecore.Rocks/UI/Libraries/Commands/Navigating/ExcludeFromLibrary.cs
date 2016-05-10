// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Libraries.ContentTrees.VirtualItems;
using Sitecore.Rocks.UI.Libraries.ItemLibraries;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.Libraries.Commands.Navigating
{
    [Command]
    public class ExcludeFromLibrary : CommandBase, IToolbarElement
    {
        public ExcludeFromLibrary()
        {
            Text = "Exclude from Library";
            Group = "Navigate";
            SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            var selection = parameter as IItemSelectionContext;
            if (selection == null)
            {
                return false;
            }

            if (selection.Items.Count() != 1)
            {
                return false;
            }

            var item = selection.Items.First() as LibraryItemTreeViewItem;
            if (item == null)
            {
                return false;
            }

            return LibraryManager.Libraries.OfType<ItemLibrary>().Any(l => l.Items.Contains(item.Item));
        }

        public override void Execute(object parameter)
        {
            var selection = parameter as IItemSelectionContext;
            if (selection == null)
            {
                return;
            }

            var item = selection.Items.First() as LibraryItemTreeViewItem;
            if (item == null)
            {
                return;
            }

            foreach (var itemLibrary in LibraryManager.Libraries.OfType<ItemLibrary>())
            {
                itemLibrary.Items.Remove(item.Item);
            }
        }
    }
}
