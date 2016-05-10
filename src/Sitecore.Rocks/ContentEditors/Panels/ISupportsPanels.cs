// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Controls;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.ContentEditors.Panels
{
    public interface ISupportsPanels
    {
        void DockFill([NotNull] string tabHeader, double priority, [NotNull] Control userControl);

        void DockInner([NotNull] Control userControl, Dock dockPosition);

        void DockOuter([NotNull] Control userControl, Dock dockPosition);
    }
}
