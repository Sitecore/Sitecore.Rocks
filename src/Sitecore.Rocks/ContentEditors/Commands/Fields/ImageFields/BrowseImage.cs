// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Fields;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.ImageFields
{
    [ToolbarElement(typeof(ContentEditorFieldContext), 2591, "Image", "Source", Text = "Browse", ElementType = RibbonElementType.LargeButton, Icon = "Resources/32x32/Open.png"), Command]
    public class BrowseImage : CommandBase, IToolbarElement
    {
        public BrowseImage()
        {
            Text = "Browse for Image...";
            Group = "Image";
            SortingValue = 10;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return false;
            }

            var field = context.Field;
            if (!(field.Control is ImageField))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return;
            }

            var field = context.Field;
            var imageField = field.Control as ImageField;
            if (imageField == null)
            {
                return;
            }

            imageField.BrowseImage(this, new RoutedEventArgs());
        }
    }
}
