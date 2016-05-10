// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Extensions.IItemSelectionContextExtensions;
using TaskDialogInterop;

namespace Sitecore.Rocks.ContentTrees.Commands.Publishing
{
    [Command(Submenu = PublishSubmenu.Name), Feature(FeatureNames.Publishing)]
    public class PublishItemAndRelatedItems : CommandBase
    {
        public PublishItemAndRelatedItems()
        {
            Text = "Publish Item and Related Items";
            Group = "PublishItem";
            SortingValue = 1050;
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

            var item = context.Items.FirstOrDefault();
            if (item == null)
            {
                return false;
            }

            if (!item.ItemUri.Site.IsSitecoreVersion(Constants.Versions.Version72, false))
            {
                return false;
            }

            if (!context.IsSameDatabase())
            {
                return false;
            }

            return context.Items.All(i => i.ItemUri.Site.CanExecute);
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var first = context.Items.FirstOrDefault();
            if (first == null)
            {
                return;
            }

            var options = new TaskDialogOptions
            {
                Title = "Publish Item and Related Items",
                CommonButtons = TaskDialogCommonButtons.OKCancel,
                MainIcon = VistaTaskDialogIcon.Information,
                Content = "Are you sure you want to publish this item and any related items?",
                AllowDialogCancellation = true
            };

            var r = TaskDialog.Show(options).Result;
            if (r != TaskDialogSimpleResult.Ok)
            {
                return;
            }

            AppHost.Statusbar.SetText("Publishing Item and Related Items");

            var items = string.Join(@",", context.Items.Select(item => item.ItemUri.ItemId.ToString()));

            ExecuteCompleted completed = (response, result) =>
            {
                foreach (var selectedItem in context.Items)
                {
                    Notifications.RaisePublishingItem(this, selectedItem.ItemUri, false, false);
                }
            };

            AppHost.Server.Publishing.PublishItemAndRelatedItems(first.ItemUri.DatabaseUri, items, "1", "0", completed);

            if (AppHost.Settings.Options.ShowJobViewer)
            {
                AppHost.Windows.OpenJobViewer(first.ItemUri.Site);
            }
        }
    }
}
