// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.Files
{
    [Command(Submenu = FilesSubmenu.Name), ToolbarElement(typeof(LayoutDesignerContext), 4310, "Rendering", "Files", Icon = "Resources/16x16/Doc-Javascript.png", ElementType = RibbonElementType.SmallButton, Text = "JavaScript")]
    public class EditJavaScriptFile : EditFileBase, IToolbarElement
    {
        public EditJavaScriptFile()
        {
            Text = "Edit JavaScript File";
            Group = "Renderings";
            SortingValue = 151;
        }

        protected override string GetPath(RenderingItem renderingItem, string filePath)
        {
            Debug.ArgumentNotNull(renderingItem, nameof(renderingItem));
            Debug.ArgumentNotNull(filePath, nameof(filePath));

            return Path.ChangeExtension(filePath, ".js");
        }
    }
}
