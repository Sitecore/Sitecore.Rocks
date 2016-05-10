// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Extensibility.Composition;

namespace Sitecore.Rocks.UI.LayoutDesigners.Designers.PageCodeDesigners
{
    [Export(typeof(BaseDesigner), Priority = 1000, CreationPolicy = CreationPolicy.Shared)]
    public class PageCodeDesigner : BaseDesigner
    {
        public PageCodeDesigner()
        {
            Name = "Page Code";
        }

        public override bool CanDesign(object parameter)
        {
            var context = parameter as RenderingContext;
            if (context == null)
            {
                return false;
            }

            if (context.RenderingTreeViewItem.Rendering.ItemId != "{DAFAFFB8-74AF-4141-A96A-70B16834CEC6}")
            {
                return false;
            }

            return !string.IsNullOrEmpty(context.RenderingTreeViewItem.Rendering.ItemUri.Site.WebRootPath);
        }

        public override FrameworkElement GetDesigner(object parameter)
        {
            var context = parameter as RenderingContext;
            if (context == null)
            {
                return null;
            }

            return new PageCodeDesignerControl(context.PageModel, context.RenderingTreeViewItem.Rendering);
        }
    }
}
