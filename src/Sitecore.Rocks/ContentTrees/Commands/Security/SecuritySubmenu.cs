// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentTrees.Commands.Security
{
    [Command(Submenu = "Tools")]
    public class SecuritySubmenu : Submenu
    {
        public SecuritySubmenu()
        {
            Text = Resources.Security;
            Group = "Tools";
            SortingValue = 3000;
            SubmenuName = "Security";
            ContextType = typeof(IItemSelectionContext);
        }
    }
}
