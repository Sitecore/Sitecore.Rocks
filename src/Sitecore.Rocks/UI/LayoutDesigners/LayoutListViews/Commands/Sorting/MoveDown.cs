// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutListViews.Commands.Sorting
{
    [Command(Submenu = SortingSubmenu.Name), ToolbarElement(typeof(LayoutDesignerContext), 4110, "Rendering", "Sorting", Icon = "Resources/32x32/navigate_down.png")]
    public class MoveDown : CommandBase, IDynamicToolbarElement
    {
        public MoveDown()
        {
            Text = Resources.MoveDown_MoveDown_Move_Down;
            SortingValue = 2000;
            Icon = new Icon("Resources/16x16/down.png");
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

            var index = layoutListView.List.SelectedIndex + 1;
            if (index >= layoutListView.List.Items.Count)
            {
                index = -1;
            }

            var selectedItems = context.SelectedItems.OfType<RenderingItem>().ToList();

            foreach (var renderingItem in selectedItems)
            {
                context.LayoutDesigner.LayoutDesignerView.RemoveRendering(renderingItem);
            }

            foreach (var renderingItem in Enumerable.Reverse(selectedItems))
            {
                if (index < 0)
                {
                    layoutListView.AddRendering(renderingItem);
                }
                else
                {
                    layoutListView.AddRendering(renderingItem, index, index);
                }
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
