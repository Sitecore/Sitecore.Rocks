// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LogViewer.Commands
{
    [Command(Submenu = "MaxItems")]
    public class MaxItems25 : MaxItemsCommand
    {
        public MaxItems25()
        {
            Text = Resources.MaxItems25_MaxItems25__25_Items;
            Group = "MaxItems";
            SortingValue = 1000;
            MaxItems = 25;
        }
    }
}
