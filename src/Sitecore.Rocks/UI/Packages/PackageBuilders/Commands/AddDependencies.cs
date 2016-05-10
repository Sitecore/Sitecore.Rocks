// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Extensions.IEnumerableExtensions;
using Sitecore.Rocks.UI.Packages.PackageBuilders.Dialogs;

namespace Sitecore.Rocks.UI.Packages.PackageBuilders.Commands
{
    [Command]
    public class AddDependencies : CommandBase
    {
        public AddDependencies()
        {
            Text = Resources.AddDependencies_AddDependencies_Add_Dependencies___;
            Group = "Adding";
            SortingValue = 20;
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
            if (selectedItems.Count == 0)
            {
                return false;
            }

            if (!selectedItems.OfType<IItem>().AllHasValue(i => i.ItemUri.DatabaseName))
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

            var items = new List<IItem>();

            foreach (var selectedItem in context.PackageBuilder.ItemList.SelectedItems)
            {
                var packageItem = selectedItem as PackageItem;
                if (packageItem == null)
                {
                    continue;
                }

                items.Add(packageItem.ItemHeader);
            }

            var d = new AddDependenciesDialog();

            d.Initialize(items);
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            var sb = new StringBuilder();

            foreach (var itemUri in d.SelectedItems)
            {
                if (sb.Length > 0)
                {
                    sb.Append('|');
                }

                sb.Append(itemUri.DatabaseName);
                sb.Append(',');
                sb.Append(itemUri.ItemId);
            }

            var itemList = sb.ToString();
            if (string.IsNullOrEmpty(itemList))
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

            var site = d.SelectedItems.First().Site;
            site.DataService.ExecuteAsync("Items.GetItemHeaders", completed, itemList);
        }
    }
}
