// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Media;

namespace Sitecore.Rocks.ContentTrees.Commands.Media
{
    [Command]
    public class UploadMedia : CommandBase
    {
        public UploadMedia()
        {
            Text = "Upload Media...";
            Group = "Media";
            SortingValue = 100;
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

            var item = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (item == null)
            {
                return false;
            }

            if (item.Item.Path != "/sitecore/media library" && string.Compare(item.Item.TemplateName, "Media folder", StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                return false;
            }

            if (!item.Item.ItemUri.Site.DataService.CanExecuteAsync("Media.Upload"))
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

            var item = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (item == null)
            {
                return;
            }

            var dialog = new OpenFileDialog
            {
                Title = "Upload Media",
                CheckFileExists = true,
                Filter = @"All files|*.*",
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            GetValueCompleted<ItemHeader> uploadCompleted = itemHeader =>
            {
                item.RefreshAndExpand(false);

                var newItem = item.Items.OfType<ItemTreeViewItem>().FirstOrDefault(i => i.ItemUri == itemHeader.ItemUri);
                if (newItem != null)
                {
                    newItem.IsSelected = true;
                    Keyboard.Focus(newItem);
                }

                AppHost.OpenContentEditor(itemHeader.ItemUri);
            };

            MediaManager.Upload(item.ItemUri.DatabaseUri, item.Item.Path, dialog.FileNames, uploadCompleted);
        }
    }
}
