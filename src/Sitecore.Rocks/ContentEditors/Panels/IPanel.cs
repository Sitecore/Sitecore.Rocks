// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.ContentEditors.Panels
{
    public interface IPanel
    {
        bool CanRender([NotNull] PanelContext context);

        void Render([NotNull] PanelContext context);
    }
}
