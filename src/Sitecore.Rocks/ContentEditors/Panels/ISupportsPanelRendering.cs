// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;

namespace Sitecore.Rocks.ContentEditors.Panels
{
    public interface ISupportsPanelRendering
    {
        void RenderPanels(ContentModel contentModel, IEnumerable<IPanel> panels);
    }
}
