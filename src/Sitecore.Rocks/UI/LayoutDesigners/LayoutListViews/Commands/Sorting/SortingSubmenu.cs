// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutListViews.Commands.Sorting
{
    [Command]
    public class SortingSubmenu : Submenu
    {
        public const string Name = "Sorting";

        public SortingSubmenu()
        {
            Text = "Sorting";
            Group = "Tasks";
            SortingValue = 6500;
            SubmenuName = Name;
            ContextType = typeof(LayoutDesignerContext);
        }
    }
}
