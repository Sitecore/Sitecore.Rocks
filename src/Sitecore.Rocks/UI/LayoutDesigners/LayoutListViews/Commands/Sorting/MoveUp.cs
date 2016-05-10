// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutListViews.Commands.Sorting
{
    [Command(Submenu = SortingSubmenu.Name), ToolbarElement(typeof(LayoutDesignerContext), 4100, "Rendering", "Sorting", Icon = "Resources/32x32/navigate_up.png")]
    public class MoveUp : CommandBase, IDynamicToolbarElement
    {
        public MoveUp()
        {
            Text = Resources.MoveUp_MoveUp_Move_Up;
            SortingValue = 1000;
            Icon = new Icon("Resources/16x16/up.png");
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

            var index = layoutListView.List.SelectedIndex - 1;
            if (index < 0)
            {
                index = 0;
            }

            var selectedItems = context.SelectedItems.OfType<RenderingItem>().ToList();

            foreach (var renderingItem in selectedItems)
            {
                context.LayoutDesigner.LayoutDesignerView.RemoveRendering(renderingItem);
            }

            foreach (var renderingItem in Enumerable.Reverse(selectedItems))
            {
                layoutListView.AddRendering(renderingItem, index, index);
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
