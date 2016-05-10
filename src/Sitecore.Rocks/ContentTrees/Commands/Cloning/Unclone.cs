// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentTrees.Commands.Cloning
{
    [Command(Submenu = CloneSubmenu.Name), Feature(FeatureNames.Cloning)]
    public class Unclone : CommandBase
    {
        public Unclone()
        {
            Text = "Unclone";
            Group = "Cloning";
            SortingValue = 2000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (context.Items.Count() != 1)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var item = context.Items.FirstOrDefault();
            if (item == null)
            {
                return;
            }

            if (AppHost.MessageBox(string.Format("Are you sure you want to unclone '{0}'?", item.Name), "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var item0 = item as ICanRefresh;
                if (item0 != null)
                {
                    item0.Refresh();
                }

                var item1 = item as ICanRefreshItem;
                if (item1 != null)
                {
                    item1.RefreshItem();
                }
            };

            item.ItemUri.Site.DataService.ExecuteAsync("Cloning.Unclone", completed, item.ItemUri.DatabaseName.ToString(), item.ItemUri.ItemId.ToString());
        }
    }
}
