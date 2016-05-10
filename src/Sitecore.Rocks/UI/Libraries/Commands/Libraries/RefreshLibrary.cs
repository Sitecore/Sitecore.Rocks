// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Libraries.ContentTrees.VirtualItems;

namespace Sitecore.Rocks.UI.Libraries.Commands.Libraries
{
    [Command]
    public class RefreshLibrary : CommandBase
    {
        public RefreshLibrary()
        {
            Text = Resources.Refresh;
            Group = "Refresh";
            SortingValue = 9999;
            Icon = new Icon("Resources/16x16/refresh.png");
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

            var dynamicFolder = item.Library as IDynamicLibrary;
            return dynamicFolder != null;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return;
            }

            var item = context.SelectedItems.FirstOrDefault() as LibraryTreeViewItem;
            if (item == null)
            {
                return;
            }

            var dynamicFolder = item.Library as IDynamicLibrary;
            if (dynamicFolder == null)
            {
                return;
            }

            dynamicFolder.Refresh();
        }
    }
}
