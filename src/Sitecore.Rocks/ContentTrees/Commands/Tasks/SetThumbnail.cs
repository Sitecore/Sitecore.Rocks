// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Thumbnails;

namespace Sitecore.Rocks.ContentTrees.Commands.Tasks
{
    [Command(Submenu = TasksSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.SetIcon, typeof(ContentTreeContext))]
    public class SetThumbnail : CommandBase
    {
        private const string ThumbnailFieldId = "{C7C26117-DBB1-42B2-AB5E-F7223845CCA3}";

        public SetThumbnail()
        {
            Text = "Set Thumbnail...";
            Group = "Fields";
            SortingValue = 3010;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (!context.Items.Any())
            {
                return false;
            }

            if (!context.Items.All(i => i is ItemTreeViewItem))
            {
                return false;
            }

            if (!context.Items.OfType<ItemTreeViewItem>().All(i => i.ItemUri.Site.DataService.CanExecuteAsync("Items.SetThumbnail")))
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

            var firstItem = context.Items.FirstOrDefault();
            if (firstItem == null)
            {
                return;
            }

            var dialog = new CreateThumbnailDialog();

            dialog.Initialize(firstItem.ItemUri.DatabaseUri, firstItem.Name);
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

                foreach (var item in context.Items)
                {
                    var itemUri = item.ItemUri;
                    var itemVersionUri = new ItemVersionUri(itemUri, LanguageManager.CurrentLanguage, Data.Version.Latest);
                    var fieldUri = new FieldUri(itemVersionUri, new FieldId(new Guid(ThumbnailFieldId)));

                    Notifications.RaiseFieldChanged(this, fieldUri, response);
                }
            };

            firstItem.ItemUri.Site.DataService.ExecuteAsync("Items.SetThumbnail", completed, firstItem.ItemUri.DatabaseName.ToString(), firstItem.ItemUri.ItemId.ToString(), ThumbnailFieldId, dialog.FileName, "1", dialog.X, dialog.Y, true);
        }
    }
}
