// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.Files
{
    [Command]
    public class FilesSubmenu : Submenu
    {
        public const string Name = "Files";

        public FilesSubmenu()
        {
            Text = "Files";
            Group = "Rendering";
            SortingValue = 5500;
            SubmenuName = Name;
            ContextType = typeof(LayoutDesignerContext);
        }
    }
}
