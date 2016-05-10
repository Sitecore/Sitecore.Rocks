// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Text;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentTrees.Commands.Editing
{
    [Command(Submenu = ClipboardSubmenu.Name), ToolbarElement(typeof(IItemSelectionContext), 1820, "Home", "Clipboard", ElementType = RibbonElementType.SmallButton)]
    public class CopyPathToClipboard : CommandBase, IToolbarElement
    {
        public CopyPathToClipboard()
        {
            Text = "Copy Item Path";
            Group = "clipboard";
            SortingValue = 1500;
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

            var itemPaths = item.ItemUri.Site.DataService.GetItemPath(item.ItemUri.DatabaseUri, item.ItemUri.ItemId.ToString());

            var sb = new StringBuilder();
            foreach (var itemPath in itemPaths.Reverse())
            {
                sb.Append('/');
                sb.Append(itemPath.Name);
            }

            AppHost.Clipboard.SetText(sb.ToString());
        }
    }
}
