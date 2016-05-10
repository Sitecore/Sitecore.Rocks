// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Text;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.UI.Packages.PackageBuilders.Dialogs;

namespace Sitecore.Rocks.UI.Packages.PackageBuilders.Commands
{
    [Command]
    public class AddHistory : CommandBase
    {
        public AddHistory()
        {
            Text = Resources.AddHistory_AddHistory_Add_Items_from_History___;
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

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as PackageBuilderContext;
            if (context == null)
            {
                return;
            }

            var d = new AddHistoryDialog();

            d.Initialize(context.PackageBuilder.Site);
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
