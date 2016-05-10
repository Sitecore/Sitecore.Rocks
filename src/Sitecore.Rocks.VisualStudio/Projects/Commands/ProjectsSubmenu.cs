// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Commands.Tools;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.Projects.Commands
{
    [Command(Submenu = ToolsSubmenu.Name)]
    public class ProjectsSubmenu : Submenu
    {
        public const string Name = "Projects";

        public ProjectsSubmenu()
        {
            Text = "Projects";
            Group = "Tools";
            SortingValue = 4500;
            SubmenuName = Name;
            ContextType = typeof(IItemSelectionContext);
        }
    }
}
