// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.Views
{
    [Command]
    public class ViewsSubmenu : Submenu
    {
        public const string Name = "Views";

        public ViewsSubmenu()
        {
            Text = "View";
            Group = "Views";
            SortingValue = 8200;
            SubmenuName = Name;
            ContextType = typeof(LayoutDesignerContext);
        }
    }
}
