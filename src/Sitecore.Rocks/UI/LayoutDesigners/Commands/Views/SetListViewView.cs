// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutListViews;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.Views
{
    [Command(Submenu = ViewsSubmenu.Name), ToolbarElement(typeof(LayoutDesignerContext), 9000, "View", "Pane", RibbonElementType.LargeToggleButton, Icon = "Resources/32x32/View-Details.png")]
    public class SetListViewView : CommandBase, IToolbarElement
    {
        public SetListViewView()
        {
            Text = "List View";
            Group = "Views";
            SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            IsChecked = context.LayoutDesigner.LayoutDesignerView is LayoutListView;

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return;
            }

            var layoutListViewTabs = new LayoutListView(context.LayoutDesigner);
            context.LayoutDesigner.SetView(layoutListViewTabs);
        }
    }
}
