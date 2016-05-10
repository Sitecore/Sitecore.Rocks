// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.UI.LayoutDesigners.Designers;

namespace Sitecore.Rocks.UI.LayoutsDesigners.Designers.SearchDataSourceDesigners
{
    [Export(typeof(BaseDesigner), Priority = 1000, CreationPolicy = CreationPolicy.Shared)]
    public class SearchDataSourceDesigner : BaseDesigner
    {
        public SearchDataSourceDesigner()
        {
            Name = "Search";
        }

        public override bool CanDesign(object parameter)
        {
            var context = parameter as RenderingContext;
            if (context == null)
            {
                return false;
            }

            return context.RenderingTreeViewItem.Rendering.ItemId == "{D47D6BFD-4F1F-4715-9FD1-5957EC0259F5}";
        }

        public override FrameworkElement GetDesigner(object parameter)
        {
            var context = parameter as RenderingContext;
            if (context == null)
            {
                return null;
            }

            return new SearchDataSourceDesignerControl(context.RenderingTreeViewItem.Rendering);
        }
    }
}
