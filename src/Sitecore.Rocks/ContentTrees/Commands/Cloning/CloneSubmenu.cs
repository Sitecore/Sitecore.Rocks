// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Commands.Tools;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentTrees.Commands.Cloning
{
    [Command(Submenu = ToolsSubmenu.Name, ExcludeFromSearch = true)]
    public class CloneSubmenu : Submenu
    {
        public const string Name = "Cloning";

        public CloneSubmenu()
        {
            Text = "Cloning";
            Group = "Tools";
            SortingValue = 4050;
            SubmenuName = Name;
            ContextType = typeof(IItemSelectionContext);
        }
    }
}
