// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.Chunks
{
    [Command]
    public class ChunksSubmenu : Submenu
    {
        public const string Name = "Chunks";

        public ChunksSubmenu()
        {
            Text = "Chunks";
            Group = "Rendering";
            SortingValue = 4990;
            SubmenuName = Name;
            ContextType = typeof(LayoutDesignerContext);
        }
    }
}
