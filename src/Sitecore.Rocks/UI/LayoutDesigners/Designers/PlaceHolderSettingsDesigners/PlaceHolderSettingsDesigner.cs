// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Extensibility.Composition;

namespace Sitecore.Rocks.UI.LayoutDesigners.Designers.PlaceHolderSettingsDesigners
{
    [Export(typeof(BaseDesigner), Priority = 1000, CreationPolicy = CreationPolicy.Shared)]
    public class PlaceHolderSettingsDesigner : BaseDesigner
    {
        public PlaceHolderSettingsDesigner()
        {
            Name = "Place Holder Settings";
        }

        public override bool CanDesign(object parameter)
        {
            return parameter is PlaceHolderContext;
        }

        public override FrameworkElement GetDesigner(object parameter)
        {
            var context = parameter as PlaceHolderContext;
            if (context == null)
            {
                return null;
            }

            return new PlaceHolderSettingsDesignerControl(context.PlaceHolderTreeViewItem);
        }
    }
}
