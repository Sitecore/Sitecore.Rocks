// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentTrees.Commands.Navigating
{
    [Command]
    public class NavigateSubmenu : Submenu
    {
        public NavigateSubmenu()
        {
            Text = Resources.NavigateSubmenu_NavigateSubmenu_Navigate_To;
            Group = "Navigate";
            SortingValue = 5000;
            SubmenuName = "Navigate";
            ContextType = typeof(IItemSelectionContext);
        }
    }
}
