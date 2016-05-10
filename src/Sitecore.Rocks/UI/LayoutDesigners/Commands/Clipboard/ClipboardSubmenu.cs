// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.Clipboard
{
    [Command]
    public class ClipboardSubmenu : Submenu
    {
        public const string Name = "Clipboard";

        public ClipboardSubmenu()
        {
            Text = "Clipboard";
            Group = "Edit";
            SortingValue = 4050;
            SubmenuName = Name;
            ContextType = typeof(LayoutDesignerContext);
        }
    }
}
