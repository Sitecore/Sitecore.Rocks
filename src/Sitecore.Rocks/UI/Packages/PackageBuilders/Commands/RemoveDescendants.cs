// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Packages.PackageBuilders.Commands
{
    [Command]
    public class RemoveDescendants : CommandBase
    {
        public RemoveDescendants()
        {
            Text = "Remove Subitems";
            Group = "Adding";
            SortingValue = 11;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as PackageBuilderContext;
            if (context == null)
            {
                return false;
            }

            if (context.Sender != context.PackageBuilder.ItemList)
            {
                return false;
            }

            var selectedItems = context.PackageBuilder.ItemList.SelectedItems;
            if (selectedItems.Count != 1)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as PackageBuilderContext;
            if (context == null)
            {
                return;
            }

            var selectedItems = context.PackageBuilder.ItemList.SelectedItems;
            if (selectedItems.Count != 1)
            {
                return;
            }

            var packageItem = context.PackageBuilder.ItemList.SelectedItem as PackageItem;
            if (packageItem == null)
            {
                return;
            }

            var modified = false;
            var path = packageItem.Path;
            var list = new List<PackageItem>(context.PackageBuilder.Items);

            foreach (var item in list)
            {
                if (item.Path == path || !item.Path.StartsWith(path, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                modified = true;
                context.PackageBuilder.Items.Remove(item);
            }

            if (!modified)
            {
                return;
            }

            context.PackageBuilder.Modified = true;
            context.PackageBuilder.ShowItemList();
        }
    }
}
