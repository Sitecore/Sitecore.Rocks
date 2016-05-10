// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Extensions.GuidExtensions;
using Sitecore.Rocks.UI;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;

namespace Sitecore.Rocks.ContentTrees.Commands.Cloning
{
    [Command(Submenu = CloneSubmenu.Name), Feature(FeatureNames.Cloning)]
    public class Clone : CommandBase
    {
        public Clone()
        {
            Text = "Clone";
            Group = "Cloning";
            SortingValue = 1000;
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

            var item = context.Items.First();

            return item.ItemUri.Site.DataService.CanExecuteAsync("Cloning.Clone");
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

            var dialog = new SelectItemDialog();
            dialog.Initialize("Clone Item To", item.ItemUri);
            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var activeTree = ActiveContext.ActiveContentTree;
                if (activeTree == null)
                {
                    return;
                }

                if (!response.IsGuid())
                {
                    return;
                }

                var itemUri = new ItemUri(item.ItemUri.DatabaseUri, new ItemId(new Guid(response)));
                activeTree.Locate(itemUri);
            };

            item.ItemUri.Site.DataService.ExecuteAsync("Cloning.Clone", completed, item.ItemUri.DatabaseName.ToString(), item.ItemUri.ItemId.ToString(), dialog.SelectedItemUri.DatabaseName.ToString(), dialog.SelectedItemUri.ItemId.ToString(), "1");
        }
    }
}
