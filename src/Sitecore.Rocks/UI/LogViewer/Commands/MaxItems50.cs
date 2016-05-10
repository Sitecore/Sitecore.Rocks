// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LogViewer.Commands
{
    [Command(Submenu = "MaxItems")]
    public class MaxItems50 : MaxItemsCommand
    {
        public MaxItems50()
        {
            Text = Resources.MaxItems50_MaxItems50__50_Items;
            Group = "MaxItems";
            SortingValue = 2000;
            MaxItems = 50;
        }
    }
}
