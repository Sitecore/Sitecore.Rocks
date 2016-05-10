// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.Files
{
    [Command(Submenu = FilesSubmenu.Name), ToolbarElement(typeof(LayoutDesignerContext), 4300, "Rendering", "Files", Icon = "Resources/32x32/Doc-XML.png", Text = "Rendering")]
    public class EditFilePath : EditFileBase, IToolbarElement
    {
        public EditFilePath()
        {
            Text = "Edit Rendering File";
            Group = "Renderings";
            SortingValue = 150;
        }

        protected override string GetPath(RenderingItem renderingItem, string filePath)
        {
            Debug.ArgumentNotNull(renderingItem, nameof(renderingItem));
            Debug.ArgumentNotNull(filePath, nameof(filePath));

            return filePath;
        }
    }
}
