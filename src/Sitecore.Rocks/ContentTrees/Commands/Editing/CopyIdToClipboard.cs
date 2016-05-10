// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentTrees.Commands.Editing
{
    [Command(Submenu = ClipboardSubmenu.Name), ToolbarElement(typeof(IItemSelectionContext), 1800, "Home", "Clipboard", ElementType = RibbonElementType.SmallButton)]
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
            var selection = parameter as IItemSelectionContext;
            if (selection == null)
            {
                return false;
            }

            return selection.Items.Count() == 1;
        }

        public override void Execute(object parameter)
        {
            var selection = parameter as IItemSelectionContext;
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
