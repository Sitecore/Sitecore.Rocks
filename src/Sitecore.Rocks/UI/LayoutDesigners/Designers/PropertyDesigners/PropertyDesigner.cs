// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;

namespace Sitecore.Rocks.UI.LayoutDesigners.Designers.PropertyDesigners
{
    /* [Export(typeof(BaseDesigner), Priority = 9000, CreationPolicy = CreationPolicy.Shared)] */

    public class PropertyDesigner : BaseDesigner
    {
        public PropertyDesigner()
        {
            Name = "Advanced";
        }

        public override bool CanDesign(object parameter)
        {
            return parameter is RenderingContext;
        }

        public override FrameworkElement GetDesigner(object parameter)
        {
            var context = parameter as RenderingContext;
            if (context == null)
            {
                return null;
            }

            return new PropertyDesignerControl(context.RenderingTreeViewItem.Rendering);
        }
    }
}
