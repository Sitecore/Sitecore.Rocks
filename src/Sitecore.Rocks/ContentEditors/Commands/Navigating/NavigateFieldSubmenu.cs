// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.ContentEditors.Commands.Navigating
{
    [Command]
    public class NavigateFieldSubmenu : Submenu
    {
        public NavigateFieldSubmenu()
        {
            Text = Resources.Navigate_To;
            Group = "Navigate";
            SortingValue = 5000;
            SubmenuName = "NavigateField";
            ContextType = typeof(ContentEditorFieldContext);
        }
    }
}
