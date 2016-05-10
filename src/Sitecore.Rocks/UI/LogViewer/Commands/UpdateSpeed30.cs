// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LogViewer.Commands
{
    [Command(Submenu = "UpdateSpeed")]
    public class UpdateSpeed30 : UpdateSpeedCommand
    {
        public UpdateSpeed30()
        {
            Text = "Every 30 seconds";
            Group = "Speed";
            SortingValue = 2000;
            UpdateSpeed = 30;
        }
    }
}
