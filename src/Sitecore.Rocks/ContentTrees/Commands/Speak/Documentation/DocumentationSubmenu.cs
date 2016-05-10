// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Commands.Tools;

namespace Sitecore.Rocks.ContentTrees.Commands.Documentation
{
    [Command(Submenu = ToolsSubmenu.Name)]
    public class DocumentationSubmenu : Submenu
    {
        public const string Name = "SPEAKDocumentation";

        public DocumentationSubmenu()
        {
            Text = "SPEAK Documentation";
            Group = "SPEAK";
            SortingValue = 4500;
            SubmenuName = Name;
            ContextType = typeof(ContentTreeContext);
        }
    }
}
