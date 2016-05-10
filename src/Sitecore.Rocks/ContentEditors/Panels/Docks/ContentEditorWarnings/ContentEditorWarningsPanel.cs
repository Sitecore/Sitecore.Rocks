// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Controls;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Panels.Docks.ContentEditorWarnings
{
    [Panel("Content Editor Warnings", 2000, EnabledByDefault = true)]
    public class ContentEditorWarningsPanel : PanelBase
    {
        public override bool CanRender(PanelContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            return context.ContentModel.IsSingle;
        }

        public override void Render(PanelContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            var control = new ContentEditorWarnings();

            control.Load(context.ContentModel);

            DockOuter(context, control, Dock.Top);
        }
    }
}
