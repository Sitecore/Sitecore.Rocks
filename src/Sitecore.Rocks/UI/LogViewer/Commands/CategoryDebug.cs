// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LogViewer.Commands
{
    [Command(Submenu = "Categories")]
    public class CategoryDebug : CategoryCommand
    {
        public CategoryDebug()
        {
            Text = "Debug";
            Group = "Categories";
            SortingValue = 4000;
            CategoryCode = "D";
        }
    }
}
