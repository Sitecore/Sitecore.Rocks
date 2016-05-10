// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.VirtualItems.RecentItems;

namespace Sitecore.Rocks.ContentTrees.Commands
{
    [Command]
    public class ClearRecent : CommandBase
    {
        public ClearRecent()
        {
            Text = Resources.Clear;
            Group = "Recent";
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

            var item = context.SelectedItems.FirstOrDefault() as RecentItemsTreeViewItemBase;
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

            var item = context.SelectedItems.FirstOrDefault() as RecentItemsTreeViewItemBase;
            if (item == null)
            {
                return;
            }

            item.Clear();
        }
    }
}
