// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LogViewer.Commands
{
    [Command(Submenu = "UpdateSpeed")]
    public class UpdateSpeed60 : UpdateSpeedCommand
    {
        public UpdateSpeed60()
        {
            Text = "Every 60 seconds";
            Group = "Speed";
            SortingValue = 3000;
            UpdateSpeed = 60;
        }
    }
}
