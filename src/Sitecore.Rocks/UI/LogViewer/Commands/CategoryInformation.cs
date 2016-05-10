// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LogViewer.Commands
{
    [Command(Submenu = "Categories")]
    public class CategoryInformation : CategoryCommand
    {
        public CategoryInformation()
        {
            Text = "Information";
            Group = "Categories";
            SortingValue = 3000;
            CategoryCode = "I";
        }
    }
}
