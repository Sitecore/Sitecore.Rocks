// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Extensibility.Composition;

namespace Sitecore.Rocks.UI.LayoutDesigners.Designers.LayoutDesigners
{
    [Export(typeof(BaseDesigner), Priority = 1000, CreationPolicy = CreationPolicy.Shared)]
    public class LayoutDesigner : BaseDesigner
    {
        public LayoutDesigner()
        {
            Name = "Layout";
        }

        public override bool CanDesign(object parameter)
        {
            return parameter is DeviceContext;
        }

        public override FrameworkElement GetDesigner(object parameter)
        {
            var context = parameter as DeviceContext;
            if (context == null)
            {
                return null;
            }

            return new LayoutDesignerControl(context.Device);
        }
    }
}
