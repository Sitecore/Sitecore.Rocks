// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentTrees.Commands.Searching
{
    [Command]
    public class SearchSubmenu : Submenu
    {
        public SearchSubmenu()
        {
            Text = Resources.SearchSubmenu_SearchSubmenu_Search;
            Group = "Navigate";
            SortingValue = 5100;
            SubmenuName = "Search";
            ContextType = typeof(IItemSelectionContext);
        }
    }
}
