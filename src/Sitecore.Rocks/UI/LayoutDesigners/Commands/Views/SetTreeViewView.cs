// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.Views
{
    [Command(Submenu = ViewsSubmenu.Name), ToolbarElement(typeof(LayoutDesignerContext), 9010, "View", "Pane", RibbonElementType.LargeToggleButton, Icon = "Resources/32x32/Tree-View.png")]
    public class SetTreeViewView : CommandBase, IToolbarElement
    {
        public SetTreeViewView()
        {
            Text = "Tree View";
            Group = "Views";
            SortingValue = 1100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            IsChecked = context.LayoutDesigner.LayoutDesignerView is LayoutTreeView;

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return;
            }

            var layoutTreeView = new LayoutTreeView(context.LayoutDesigner);
            context.LayoutDesigner.SetView(layoutTreeView);
        }
    }
}
