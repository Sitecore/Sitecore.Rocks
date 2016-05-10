// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Extensions.IItemSelectionContextExtensions;
using Sitecore.Rocks.UI.Publishing.Dialogs;

namespace Sitecore.Rocks.ContentTrees.Commands.Publishing
{
    public abstract class PublishItemAndDependenciesCommand : CommandBase
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

            var first = context.Items.FirstOrDefault();
            if (first == null)
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

            var d = new PublishItemDialog();
            d.Initialize(context.Items, Resources.PublishItemCommand_Execute_Publishing, PublishingText);
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            if (!d.SelectedItems.Any())
            {
                return;
            }

            AppHost.Statusbar.SetText(PublishingText);

            IItem lastItem = null;

            first.ItemUri.Site.DataService.PublishItem(d.SelectedItems, Deep, CompareRevisisions);

            foreach (var selectedItem in context.Items)
            {
                Notifications.RaisePublishingItem(this, selectedItem.ItemUri, Deep, CompareRevisisions);

                lastItem = selectedItem;
            }

            if (AppHost.Settings.Options.ShowJobViewer && lastItem != null)
            {
                AppHost.Windows.OpenJobViewer(lastItem.ItemUri.Site);
            }
        }
    }
}
