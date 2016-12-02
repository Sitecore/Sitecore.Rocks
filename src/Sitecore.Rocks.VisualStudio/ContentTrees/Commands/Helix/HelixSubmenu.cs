// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentTrees.Commands.Helix
{
    [Command(ExcludeFromSearch = true)]
    public class HelixSubmenu : Submenu
    {
        public const string Name = "Helix";

        public HelixSubmenu()
        {
            Text = "Helix";
            Group = "Tools";
            SortingValue = 4800;
            SubmenuName = Name;
            ContextType = typeof(ISiteSelectionContext);
        }
    }
}
