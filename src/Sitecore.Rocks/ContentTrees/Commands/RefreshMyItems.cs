// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.VirtualItems.MyItems;

namespace Sitecore.Rocks.ContentTrees.Commands
{
    [Command]
    public class RefreshMyItems : CommandBase
    {
        public RefreshMyItems()
        {
            Text = Resources.RefreshMyItems_RefreshMyItems_Refresh;
            Group = "Refresh";
            SortingValue = 9999;
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

            var item = context.SelectedItems.FirstOrDefault() as MyItemsTreeViewItemBase;
            if (item == null)
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

            var item = context.SelectedItems.FirstOrDefault() as MyItemsTreeViewItemBase;
            if (item == null)
            {
                return;
            }

            item.Refresh();
        }
    }
}
