// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentTrees.Commands.Editing
{
    [Command(Submenu = ClipboardSubmenu.Name), ToolbarElement(typeof(IItemSelectionContext), 1830, "Home", "Clipboard", ElementType = RibbonElementType.SmallButton)]
    public class CopyIconPathToClipboard : CommandBase, IToolbarElement
    {
        public CopyIconPathToClipboard()
        {
            Text = "Copy Icon Path";
            Group = "clipboard";
            SortingValue = 1600;
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

            return true;
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

            var iconPath = item.Icon.IconPath;
            if (iconPath.StartsWith("/temp/IconCache/"))
            {
                iconPath = iconPath.Mid(16);
            }

            AppHost.Clipboard.SetText(iconPath);
        }
    }
}
