// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.Clipboard
{
    [Command(Submenu = ClipboardSubmenu.Name), ToolbarElement(typeof(LayoutDesignerContext), 4500, "Rendering", "Clipboard", ElementType = RibbonElementType.SmallButton)]
    public class CopyIdToClipboard : CommandBase, IToolbarElement
    {
        public CopyIdToClipboard()
        {
            Text = "Copy Item ID";
            Group = "clipboard";
            SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            var selection = parameter as LayoutDesignerContext;
            if (selection == null)
            {
                return false;
            }

            return selection.Items.Count() == 1;
        }

        public override void Execute(object parameter)
        {
            var selection = parameter as LayoutDesignerContext;
            if (selection == null)
            {
                return;
            }

            var item = selection.Items.FirstOrDefault();
            if (item == null)
            {
                return;
            }

            AppHost.Clipboard.SetText(item.ItemUri.ItemId.ToString());
        }
    }
}
