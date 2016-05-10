// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Extensibility.Composition;

namespace Sitecore.Rocks.UI.LayoutDesigners.Designers.QueryDataSourceBuilders
{
    [Export(typeof(BaseDesigner), Priority = 1000, CreationPolicy = CreationPolicy.Shared)]
    public class QueryDataSourceDesigner : BaseDesigner
    {
        public QueryDataSourceDesigner()
        {
            Name = "Query";
        }

        public override bool CanDesign(object parameter)
        {
            var context = parameter as RenderingContext;
            if (context == null)
            {
                return false;
            }

            return context.RenderingTreeViewItem.Rendering.ItemId == "{33A02142-2DBD-4034-AEA2-BDF215EB2451}";
        }

        public override FrameworkElement GetDesigner(object parameter)
        {
            var context = parameter as RenderingContext;
            if (context == null)
            {
                return null;
            }

            return new QueryDataSourceDesignerControl(context.RenderingTreeViewItem.Rendering);
        }
    }
}
