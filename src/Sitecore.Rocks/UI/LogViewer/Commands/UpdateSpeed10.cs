// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LogViewer.Commands
{
    [Command(Submenu = "UpdateSpeed")]
    public class UpdateSpeed10 : UpdateSpeedCommand
    {
        public UpdateSpeed10()
        {
            Text = Resources.UpdateSpeed10_UpdateSpeed10_Every_10_seconds;
            Group = "Speed";
            SortingValue = 1000;
            UpdateSpeed = 10;
        }
    }
}
