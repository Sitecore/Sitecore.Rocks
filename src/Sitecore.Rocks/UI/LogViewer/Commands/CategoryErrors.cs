// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LogViewer.Commands
{
    [Command(Submenu = "Categories")]
    public class CategoryErrors : CategoryCommand
    {
        public CategoryErrors()
        {
            Text = "Errors";
            Group = "Categories";
            SortingValue = 1000;
            CategoryCode = "E";
        }
    }
}
