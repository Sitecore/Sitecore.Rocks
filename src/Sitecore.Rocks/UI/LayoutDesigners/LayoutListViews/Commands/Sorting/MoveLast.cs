// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutListViews.Commands.Sorting
{
    [Command(Submenu = SortingSubmenu.Name), ToolbarElement(typeof(LayoutDesignerContext), 4130, "Rendering", "Sorting", Icon = "Resources/16x16/navigate_down2.png", ElementType = RibbonElementType.SmallButton)]
    public class MoveLast : CommandBase, IDynamicToolbarElement
    {
        public MoveLast()
        {
            Text = Resources.MoveLast_MoveLast_Move_Last;
            SortingValue = 4000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            var selectedItems = context.SelectedItems;
            if (!selectedItems.Any())
            {
                return false;
            }

            var tabsLayoutDesignerView = context.LayoutDesigner.LayoutDesignerView as LayoutListView;
            if (tabsLayoutDesignerView == null)
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

            var tabsLayoutDesignerView = context.LayoutDesigner.LayoutDesignerView as LayoutListView;
            if (tabsLayoutDesignerView == null)
            {
                return;
            }

            var layoutListView = tabsLayoutDesignerView.GetActiveListView();
            if (layoutListView == null)
            {
                return;
            }

            var selectedItems = context.SelectedItems.OfType<RenderingItem>().ToList();

            foreach (var renderingItem in selectedItems)
            {
                context.LayoutDesigner.LayoutDesignerView.RemoveRendering(renderingItem);
            }

            foreach (var renderingItem in Enumerable.Reverse(selectedItems))
            {
                layoutListView.AddRendering(renderingItem, -1, -1);
            }

            layoutListView.List.SelectedItem = selectedItems.FirstOrDefault();
            context.LayoutDesigner.Modified = true;
        }

        bool IDynamicToolbarElement.CanRender(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            return context != null && context.LayoutDesigner.LayoutDesignerView is LayoutListView;
        }
    }
}
