// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Libraries.ContentTrees.VirtualItems;
using Sitecore.Rocks.UI.Libraries.ItemLibraries;

namespace Sitecore.Rocks.UI.Libraries.Commands.Libraries
{
    [Command]
    public class NewItemLibrary : CommandBase
    {
        public NewItemLibrary()
        {
            Text = "New Item Library";
            Group = "New";
            SortingValue = 1000;
            Icon = new Icon("Sitecore.Rocks", "/Resources/16x16/newfolder.png");
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

            var item = context.SelectedItems.FirstOrDefault() as LibrariesRootTreeViewItem;
            return item != null;
        }

        public override void Execute(object parameter)
        {
            LibraryManager.AddNew((fileName, name) => new ItemLibrary(fileName, name));
        }
    }
}
