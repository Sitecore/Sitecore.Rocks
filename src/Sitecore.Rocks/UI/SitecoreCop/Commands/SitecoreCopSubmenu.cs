// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.UI.SitecoreCop.Commands
{
    public class SitecoreCopSubmenu : Submenu
    {
        public const string Name = "SitecoreCop";

        public SitecoreCopSubmenu()
        {
            Text = "SitecoreCop";
            Group = "Cops";
            SortingValue = 9500;
            SubmenuName = Name;
            ContextType = typeof(IItemSelectionContext);
        }
    }
}
