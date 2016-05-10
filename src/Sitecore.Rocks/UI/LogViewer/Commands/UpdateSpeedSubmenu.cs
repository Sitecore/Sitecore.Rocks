// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LogViewer.Commands
{
    [Command]
    public class UpdateSpeedSubmenu : Submenu
    {
        public UpdateSpeedSubmenu()
        {
            Text = Resources.UpdateSpeedSubmenu_UpdateSpeedSubmenu_Update_Speed;
            Group = "Settings";
            SortingValue = 4000;
            SubmenuName = "UpdateSpeed";
            ContextType = typeof(LogViewerContext);
        }
    }
}
