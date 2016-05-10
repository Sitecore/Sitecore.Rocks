// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LogViewer.Commands
{
    [Command]
    public class MaxItemsSubmenu : Submenu
    {
        public MaxItemsSubmenu()
        {
            Text = Resources.MaxItemsSubmenu_MaxItemsSubmenu_Max_Items;
            Group = "Settings";
            SortingValue = 5000;
            SubmenuName = "MaxItems";
            ContextType = typeof(LogViewerContext);
        }
    }
}
