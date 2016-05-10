// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.Views
{
    [Command(Submenu = ViewsSubmenu.Name), ToolbarElement(typeof(LayoutDesignerContext), 9100, "View", "Show", ElementType = RibbonElementType.CheckBox)]
    public class ShowRibbon : CommandBase, IToolbarElement
    {
        public ShowRibbon()
        {
            Text = "Ribbon";
            Group = "Menu";
            SortingValue = 3000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            IsChecked = AppHost.Settings.GetBool("LayoutDesigner", "ShowRibbon", false);

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return;
            }

            AppHost.Settings.SetBool("LayoutDesigner", "ShowRibbon", !AppHost.Settings.GetBool("LayoutDesigner", "ShowRibbon", false));
            context.LayoutDesigner.ShowRibbon();
        }
    }
}
