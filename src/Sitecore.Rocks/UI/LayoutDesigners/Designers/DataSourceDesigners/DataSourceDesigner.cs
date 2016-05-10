// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Extensibility.Composition;

namespace Sitecore.Rocks.UI.LayoutDesigners.Designers.DataSourceDesigners
{
    [Export(typeof(BaseDesigner), Priority = 6000, CreationPolicy = CreationPolicy.Shared)]
    public class DataSourceDesigner : BaseDesigner
    {
        public DataSourceDesigner()
        {
            Name = "Data Source";
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

            return new DataSourceDesignerControl(context.RenderingTreeViewItem.Rendering);
        }
    }
}
