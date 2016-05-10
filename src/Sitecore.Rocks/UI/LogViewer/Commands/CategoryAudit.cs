// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LogViewer.Commands
{
    [Command(Submenu = "Categories")]
    public class CategoryAudit : CategoryCommand
    {
        public CategoryAudit()
        {
            Text = "Audit";
            Group = "Categories";
            SortingValue = 5000;
            CategoryCode = "A";
        }
    }
}
