// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentTrees.Commands.Editing
{
    [Command(Submenu = ClipboardSubmenu.Name), ToolbarElement(typeof(IItemSelectionContext), 1810, "Home", "Clipboard", ElementType = RibbonElementType.SmallButton)]
    public class CopyTemplateIdToClipboard : CommandBase, IToolbarElement
    {
        public CopyTemplateIdToClipboard()
        {
            Text = "Copy Template ID";
            Group = "clipboard";
            SortingValue = 1200;
        }

        public override bool CanExecute(object parameter)
        {
            var selection = parameter as IItemSelectionContext;
            if (selection == null)
            {
                return false;
            }

            if (selection.Items.Count() != 1)
            {
                return false;
            }

            var templatedItem = selection.Items.FirstOrDefault() as ITemplatedItem;
            if (templatedItem == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var selection = parameter as IItemSelectionContext;
            if (selection == null)
            {
                return;
            }

            var item = selection.Items.FirstOrDefault() as ITemplatedItem;
            if (item == null)
            {
                return;
            }

            AppHost.Clipboard.SetText(item.TemplateId.ToString());
        }
    }
}
