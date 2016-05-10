// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.JobViewer.Commands
{
    [Command(Submenu = "UpdateSpeed")]
    public class UpdateSpeed120 : UpdateSpeedCommand
    {
        public UpdateSpeed120()
        {
            Text = "Every 120 seconds";
            Group = "Speed";
            SortingValue = 4000;
            UpdateSpeed = 120;
        }
    }
}
