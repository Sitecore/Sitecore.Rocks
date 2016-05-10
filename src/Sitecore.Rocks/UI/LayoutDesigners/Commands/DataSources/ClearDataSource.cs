// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.DataSources
{
    [Command(Submenu = DataSourceSubmenu.Name), ToolbarElement(typeof(LayoutDesignerContext), 4230, "Rendering", "Data Sources", Icon = "Resources/16x16/delete.png", ElementType = RibbonElementType.SmallButton)]
    public class ClearDataSource : CommandBase, IToolbarElement
    {
        public ClearDataSource()
        {
            Text = "Clear";
            Group = "DataSource";
            SortingValue = 2000;
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

            return !string.IsNullOrEmpty(item.DataSource);
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

            renderingTreeViewItem.DataSource = string.Empty;
        }
    }
}
