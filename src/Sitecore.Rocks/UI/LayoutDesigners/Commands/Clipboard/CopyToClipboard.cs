// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using System.Xml;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.Clipboard
{
    [Command(Submenu = ClipboardSubmenu.Name), ToolbarElement(typeof(LayoutDesignerContext), 1003, "Home", "Editing", Icon = "Resources/16x16/copy.png", ElementType = RibbonElementType.SmallButton)]
    public class CopyToClipboard : CommandBase, IToolbarElement
    {
        public CopyToClipboard()
        {
            Text = "Copy";
            Group = "Edit";
            SortingValue = 4000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            return context.SelectedItems.OfType<RenderingItem>().Any();
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return;
            }

            var selectedItems = context.SelectedItems.OfType<RenderingItem>();

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("renderings");

            foreach (var renderingItem in selectedItems)
            {
                renderingItem.Write(output, true);
            }

            output.WriteEndElement();

            var text = @"Sitecore.Clipboard.Renderings:" + writer;

            AppHost.Clipboard.SetText(text);

            context.LayoutDesigner.UpdateRibbon(context.LayoutDesigner.LayoutDesignerView);
        }
    }
}
