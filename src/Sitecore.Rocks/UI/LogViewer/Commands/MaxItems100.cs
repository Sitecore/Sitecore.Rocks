// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LogViewer.Commands
{
    [Command(Submenu = "MaxItems")]
    public class MaxItems100 : MaxItemsCommand
    {
        public MaxItems100()
        {
            Text = Resources.MaxItems100_MaxItems100__100_Items;
            Group = "MaxItems";
            SortingValue = 3000;
            MaxItems = 100;
        }
    }
}
