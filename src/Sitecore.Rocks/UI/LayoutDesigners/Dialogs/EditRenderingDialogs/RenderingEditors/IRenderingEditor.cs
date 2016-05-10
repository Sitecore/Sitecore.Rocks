// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs.EditRenderingDialogs.RenderingEditors
{
    public interface IRenderingEditor
    {
        [NotNull]
        RenderingItem RenderingItem { get; set; }

        void Update();
    }
}
