// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Commands.Tasks;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentTrees.Commands.Sorting
{
    [Command(Submenu = TasksSubmenu.Name)]
    public class SortingSubmenu : Submenu
    {
        public SortingSubmenu()
        {
            Text = Resources.SortingSubmenu_SortingSubmenu_Sorting;
            Group = "Sorting";
            SortingValue = 4020;
            Icon = new Icon("Resources/16x16/sorting.png");
            SubmenuName = "Sorting";
            ContextType = typeof(ContentTreeContext);
        }
    }
}
