// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.DataSources
{
    [Command(Submenu = DataSourceSubmenu.Name), ToolbarElement(typeof(LayoutDesignerContext), 4200, "Rendering", "Data Sources", Icon = "Resources/32x32/Open.png")]
    public class BrowseDataSource : CommandBase, IToolbarElement
    {
        public BrowseDataSource()
        {
            Text = "Browse";
            Group = "DataSource";
            SortingValue = 1200;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return false;
            }

            var item = context.SelectedItems.FirstOrDefault() as RenderingItem;
            if (item == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return;
            }

            var renderingTreeViewItem = context.SelectedItems.FirstOrDefault() as RenderingItem;
            if (renderingTreeViewItem == null)
            {
                return;
            }

            var itemUri = context.LayoutDesigner.FieldUris.First().ItemVersionUri.ItemUri;

            var dialog = new SelectItemDialog();
            dialog.Initialize("Set Data Source", itemUri);
            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            renderingTreeViewItem.DataSource = dialog.SelectedItemUri.ItemId.ToString();
        }
    }
}
