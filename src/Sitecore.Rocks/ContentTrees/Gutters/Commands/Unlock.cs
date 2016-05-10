// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Text;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Gutters.Commands
{
    [Command]
    public class Unlock : CommandBase
    {
        public Unlock()
        {
            Text = Resources.Unlock_Unlock_Unlock;
            Group = "Locking";
            SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as GutterContext;
            if (context == null)
            {
                return false;
            }

            if (!context.SelectedItems.All(IsLockedItem))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as GutterContext;
            if (context == null)
            {
                return;
            }

            var itemList = new StringBuilder();
            ItemTreeViewItem first = null;

            foreach (var itemTreeViewItem in context.SelectedItems.OfType<ItemTreeViewItem>())
            {
                if (first == null)
                {
                    first = itemTreeViewItem;
                }
                else
                {
                    itemList.Append('|');
                }

                itemList.Append(itemTreeViewItem.ItemUri.ItemId);
            }

            if (first == null)
            {
                return;
            }

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                foreach (var item in context.SelectedItems.OfType<ItemTreeViewItem>())
                {
                    item.UpdateGutters();
                }
            };

            first.ItemUri.Site.DataService.ExecuteAsync("Security.Unlock", completed, first.ItemUri.DatabaseName.Name, itemList.ToString());
        }

        private bool IsLockedItem([NotNull] BaseTreeViewItem baseTreeViewItem)
        {
            Debug.ArgumentNotNull(baseTreeViewItem, nameof(baseTreeViewItem));

            var item = baseTreeViewItem as ItemTreeViewItem;
            if (item == null)
            {
                return false;
            }

            foreach (var gutter in item.Item.Gutters)
            {
                if (gutter.Icon.IconPath.EndsWith(@"Network/16x16/lock.png", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (gutter.Icon.IconPath.EndsWith(@"People/16x16/user1_lock.png", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
