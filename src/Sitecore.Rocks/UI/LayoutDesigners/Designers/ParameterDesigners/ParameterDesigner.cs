// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using Sitecore.Rocks.Extensibility.Composition;

namespace Sitecore.Rocks.UI.LayoutDesigners.Designers.ParameterDesigners
{
    [Export(typeof(BaseDesigner), Priority = 1000, CreationPolicy = CreationPolicy.Shared)]
    public class ParameterDesigner : BaseDesigner
    {
        public ParameterDesigner()
        {
            Name = "Parameters";
        }

        public override bool CanDesign(object parameter)
        {
            var context = parameter as RenderingContext;
            if (context == null)
            {
                return false;
            }

            var rendering = context.RenderingTreeViewItem.Rendering;

            if (rendering.DynamicProperties.Count == 0)
            {
                return false;
            }

            if (rendering.DynamicProperties.Count == 1 && rendering.DynamicProperties.First().Name == "Id")
            {
                return false;
            }

            return true;
        }

        public override FrameworkElement GetDesigner(object parameter)
        {
            var context = parameter as RenderingContext;
            if (context == null)
            {
                return null;
            }

            return new ParameterDesignerControl(context.PageModel, context.RenderingTreeViewItem.Rendering);
        }
    }
}
