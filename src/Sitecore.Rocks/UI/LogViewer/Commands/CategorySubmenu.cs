// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LogViewer.Commands
{
    [Command]
    public class CategorySubmenu : Submenu
    {
        public CategorySubmenu()
        {
            Text = Resources.CategorySubmenu_CategorySubmenu_Categories;
            Group = "Settings";
            SortingValue = 4000;
            SubmenuName = "Categories";
            ContextType = typeof(LogViewerContext);
        }
    }
}
