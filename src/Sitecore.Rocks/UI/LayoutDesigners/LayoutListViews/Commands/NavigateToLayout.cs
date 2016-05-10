// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Input;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.LayoutDesigners.Commands.Navigate;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutListViews.Commands
{
    [Command(Submenu = NavigateSubmenu.Name), ToolbarElement(typeof(LayoutDesignerContext), 5000, "Layout", "Navigate", Icon = "Resources/32x32/Nudge-Left.png", Text = "Layout")]
    public class NavigateToLayout : CommandBase, IDynamicToolbarElement
    {
        public NavigateToLayout()
        {
            Text = "Layout";
            Group = "Navigate";
            SortingValue = 2000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            var tree = ActiveContext.ActiveContentTree;
            if (tree == null)
            {
                return false;
            }

            var tabsLayoutDesignerView = context.LayoutDesigner.LayoutDesignerView as LayoutListView;
            if (tabsLayoutDesignerView == null)
            {
                return false;
            }

            var layoutListView = tabsLayoutDesignerView.GetActiveListView();
            if (layoutListView == null)
            {
                return false;
            }

            var layoutUri = layoutListView.LayoutUri;
            if (layoutUri == ItemUri.Empty)
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

            var contentTree = ActiveContext.ActiveContentTree;
            if (contentTree == null)
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

            var layoutUri = layoutListView.LayoutUri;
            if (layoutUri == ItemUri.Empty)
            {
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                AppHost.OpenContentEditor(layoutUri);
            }
            else
            {
                contentTree.Locate(layoutUri);
            }
        }

        bool IDynamicToolbarElement.CanRender(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            return context != null && context.LayoutDesigner.LayoutDesignerView is LayoutListView;
        }
    }
}
