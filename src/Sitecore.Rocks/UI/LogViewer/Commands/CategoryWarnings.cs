// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LogViewer.Commands
{
    [Command(Submenu = "Categories")]
    public class CategoryWarnings : CategoryCommand
    {
        public CategoryWarnings()
        {
            Text = "Warnings";
            Group = "Categories";
            SortingValue = 2000;
            CategoryCode = "W";
        }
    }
}
