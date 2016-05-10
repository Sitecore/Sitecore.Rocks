// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.IO;
using Sitecore.Rocks.Rules;

namespace Sitecore.Rocks.ContentTrees.Commands.Serialization
{
    [Command(Submenu = SerializationSubmenu.Name), RuleAction("open serialization file in Windows Explorer", "Serialization"), Feature(FeatureNames.Serialization)]
    public class OpenInWindowsExplorer : CommandBase, IRuleAction
    {
        public OpenInWindowsExplorer()
        {
            Text = Resources.OpenInWindowsExplorer_OpenInWindowsExplorer_Open_in_Windows_Explorer;
            Group = "Open";
            SortingValue = 9000;
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

            var item = context.Items.FirstOrDefault() as ItemTreeViewItem;
            if (item == null)
            {
                return false;
            }

            if (!item.Site.DataService.CanExecuteAsync("Serialization.GetItemSerializationPath"))
            {
                return false;
            }

            return !string.IsNullOrEmpty(item.ItemUri.Site.WebRootPath);
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var item = context.Items.FirstOrDefault() as ItemTreeViewItem;
            if (item == null)
            {
                return;
            }

            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                if (string.IsNullOrEmpty(response))
                {
                    AppHost.MessageBox(Resources.OpenInWindowsExplorer_Execute_There_is_no_file_associated_with_this_item_, Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                File.OpenInWindowsExplorer(response);
            };

            var itemUri = item.ItemUri;

            itemUri.Site.DataService.ExecuteAsync("Serialization.GetItemSerializationPath", callback, itemUri.ItemId.ToString(), itemUri.DatabaseUri.DatabaseName.ToString());
        }
    }
}
