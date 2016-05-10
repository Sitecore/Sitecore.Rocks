// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.Navigate
{
    [Command]
    public class NavigateSubmenu : Submenu
    {
        public const string Name = "Navigate";

        public NavigateSubmenu()
        {
            Text = "Navigate";
            Group = "Tasks";
            SortingValue = 5750;
            SubmenuName = Name;
            ContextType = typeof(LayoutDesignerContext);
        }
    }
}
