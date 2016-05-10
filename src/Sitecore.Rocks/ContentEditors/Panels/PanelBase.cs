// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Panels
{
    public abstract class PanelBase : IPanel
    {
        public abstract bool CanRender(PanelContext context);

        public abstract void Render(PanelContext context);

        protected void AddTab([NotNull] PanelContext context, [NotNull] string tabHeader, double priority, [NotNull] Control userControl)
        {
            Debug.ArgumentNotNull(context, nameof(context));
            Debug.ArgumentNotNull(tabHeader, nameof(tabHeader));
            Debug.ArgumentNotNull(userControl, nameof(userControl));

            var i = context.Skin as ISupportsPanels;
            if (i != null)
            {
                i.DockFill(tabHeader, priority, userControl);
            }
        }

        [UsedImplicitly]
        protected void DockInner([NotNull] PanelContext context, [NotNull] Control userControl, Dock dockPosition)
        {
            Debug.ArgumentNotNull(context, nameof(context));
            Debug.ArgumentNotNull(userControl, nameof(userControl));

            var i = context.Skin as ISupportsPanels;
            if (i != null)
            {
                i.DockInner(userControl, dockPosition);
            }
        }

        [UsedImplicitly]
        protected void DockOuter([NotNull] PanelContext context, [NotNull] Control userControl, Dock dockPosition)
        {
            Debug.ArgumentNotNull(context, nameof(context));
            Debug.ArgumentNotNull(userControl, nameof(userControl));

            var i = context.Skin as ISupportsPanels;
            if (i != null)
            {
                i.DockOuter(userControl, dockPosition);
            }
        }
    }
}
