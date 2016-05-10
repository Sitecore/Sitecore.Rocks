// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;

namespace Sitecore.Rocks.UI.Packages.PackageBuilders.Commands
{
    [Command]
    public class AddDescendants : CommandBase
    {
        public AddDescendants()
        {
            Text = Resources.AddDescendants_AddDescendants_Add_Subitems;
            Group = "Adding";
            SortingValue = 10;
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

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!context.PackageBuilder.InternalAddItems(response, result))
                {
                    return;
                }

                context.PackageBuilder.Modified = true;
                context.PackageBuilder.ShowItemList();
            };

            packageItem.ItemUri.Site.DataService.ExecuteAsync("Items.GetDescendants", completed, packageItem.ItemUri.ItemId.ToString(), packageItem.ItemUri.DatabaseName.Name);
        }
    }
}
