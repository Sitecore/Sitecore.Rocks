// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.Clipboard
{
    [Command(Submenu = ClipboardSubmenu.Name), ToolbarElement(typeof(LayoutDesignerContext), 4500, "Rendering", "Clipboard", ElementType = RibbonElementType.SmallButton)]
    public class CopyControlIdToClipboard : CommandBase, IToolbarElement
    {
        public CopyControlIdToClipboard()
        {
            Text = "Copy Rendering ID";
            Group = "clipboard";
            SortingValue = 1600;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            if (context.Items.Count() != 1)
            {
                return false;
            }

            var rendering = context.SelectedItem as RenderingItem;
            if (rendering == null)
            {
                return false;
            }

            return !string.IsNullOrEmpty(rendering.GetControlId());
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return;
            }

            if (context.Items.Count() != 1)
            {
                return;
            }

            var rendering = context.SelectedItem as RenderingItem;
            if (rendering == null)
            {
                return;
            }

            AppHost.Clipboard.SetText(rendering.GetControlId());
        }
    }
}
