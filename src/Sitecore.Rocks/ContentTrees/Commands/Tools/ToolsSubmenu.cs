// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentTrees.Commands.Tools
{
    [Command]
    public class ToolsSubmenu : Submenu
    {
        public const string Name = "Tools";

        public ToolsSubmenu()
        {
            Text = Resources.ToolsSubmenu_ToolsSubmenu_Tools;
            Group = "Tools";
            SortingValue = 4500;
            SubmenuName = Name;
            ContextType = typeof(IItemSelectionContext);
        }
    }
}
