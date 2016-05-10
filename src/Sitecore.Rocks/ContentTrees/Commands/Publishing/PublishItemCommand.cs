// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Extensions.IItemSelectionContextExtensions;
using Sitecore.Rocks.UI.Publishing.Dialogs;

namespace Sitecore.Rocks.ContentTrees.Commands.Publishing
{
    public abstract class PublishItemCommand : CommandBase
    {
        protected bool CompareRevisisions { get; set; }

        protected bool Deep { get; set; }

        protected string PublishingText { get; set; }

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

            if (!context.IsSameDatabase())
            {
                return false;
            }

            if (!context.Items.All(i => i.ItemUri.Site.DataService.CanExecuteAsync("Publishing.PublishItem")))
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

            AppHost.Statusbar.SetText(PublishingText);

            IItem lastItem = null;

            context.Items.First().ItemUri.Site.DataService.PublishItem(context.Items.Select(i => i.ItemUri), Deep, CompareRevisisions);

            foreach (var item in context.Items)
            {
                Notifications.RaisePublishingItem(this, item.ItemUri, Deep, CompareRevisisions);

                lastItem = item;
            }

            if (lastItem == null || AppHost.Settings.Options.HidePublishingDialog)
            {
                return;
            }

            var d = new PublishDialog
            {
                Caption = Resources.PublishItemCommand_Execute_Publishing,
                PublishingText = PublishingText
            };

            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            if (AppHost.Settings.Options.ShowJobViewer)
            {
                AppHost.Windows.OpenJobViewer(lastItem.ItemUri.Site);
            }
        }
    }
}
