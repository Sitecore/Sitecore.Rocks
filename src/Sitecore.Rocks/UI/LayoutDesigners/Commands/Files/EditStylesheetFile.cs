// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.Files
{
    [Command(Submenu = FilesSubmenu.Name), ToolbarElement(typeof(LayoutDesignerContext), 4320, "Rendering", "Files", Icon = "Resources/16x16/Doc-CSS-2.png", ElementType = RibbonElementType.SmallButton, Text = "Stylesheet")]
    public class EditStylesheetFile : EditFileBase, IToolbarElement
    {
        public EditStylesheetFile()
        {
            Text = "Edit Stylesheet File";
            Group = "Renderings";
            SortingValue = 152;
        }

        protected override string GetPath(RenderingItem renderingItem, string filePath)
        {
            Debug.ArgumentNotNull(renderingItem, nameof(renderingItem));
            Debug.ArgumentNotNull(filePath, nameof(filePath));

            return Path.ChangeExtension(filePath, ".css");
        }
    }
}
