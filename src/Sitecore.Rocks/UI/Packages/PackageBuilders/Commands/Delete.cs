// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.UI.Packages.PackageBuilders.Commands
{
    [Command]
    public class Delete : CommandBase
    {
        public Delete()
        {
            Text = "Remove";
            Group = "Edit";
            SortingValue = 30;
            Icon = new Icon("Resources/16x16/delete.png");
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as PackageBuilderContext;
            if (context == null)
            {
                return false;
            }

            if (context.Sender == context.PackageBuilder.ItemList)
            {
                var selectedItems = context.PackageBuilder.ItemList.SelectedItems;
                if (selectedItems.Count == 0)
                {
                    return false;
                }

                return true;
            }

            if (context.Sender == context.PackageBuilder.FileList)
            {
                var selectedItems = context.PackageBuilder.FileList.SelectedItems;
                if (selectedItems.Count == 0)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as PackageBuilderContext;
            if (context == null)
            {
                return;
            }

            if (context.Sender == context.PackageBuilder.ItemList)
            {
                context.PackageBuilder.RemoveSelectedItems();
            }

            if (context.Sender == context.PackageBuilder.FileList)
            {
                context.PackageBuilder.RemoveSelectedFiles();
            }
        }
    }
}
