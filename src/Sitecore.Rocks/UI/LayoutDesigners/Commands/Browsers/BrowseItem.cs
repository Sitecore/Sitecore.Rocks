// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Text;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.Browsers
{
    [Command, ToolbarElement(typeof(LayoutDesignerContext), 3200, "Home", "Preview", Icon = "Resources/32x32/Slide-View.png")]
    public class BrowseItem : CommandBase, IToolbarElement
    {
        public BrowseItem()
        {
            Text = "Preview";
            Group = "Preview";
            SortingValue = 3000;
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is LayoutDesignerContext;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return;
            }

            var fieldUri = context.LayoutDesigner.FieldUris.FirstOrDefault();
            if (fieldUri == null)
            {
                SelectItemToPreview(context);
                return;
            }

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var element = response.ToXElement();
                if (element == null)
                {
                    return;
                }

                var itemHeader = ItemHeader.Parse(fieldUri.DatabaseUri, element);

                if (itemHeader.Name != "__Standard Values")
                {
                    Browse(itemHeader.ItemUri);
                    return;
                }

                SelectItemToPreview(context);
            };

            AppHost.Server.Items.GetItemHeader(fieldUri.ItemVersionUri.ItemUri, completed);
        }

        private void Browse([NotNull] ItemUri itemUri)
        {
            var path = new UrlString(@"/");

            path["sc_itemid"] = itemUri.ItemId.ToString();
            path["sc_mode"] = @"preview";

            AppHost.Browsers.Navigate(itemUri.Site, path.ToString());
        }

        private void SelectItemToPreview(LayoutDesignerContext context)
        {
            var dialog = new SelectItemDialog();
            dialog.Initialize("Preview", context.LayoutDesigner.DatabaseUri, "/sitecore/content/Home");
            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            Browse(dialog.SelectedItemUri);
        }
    }
}
