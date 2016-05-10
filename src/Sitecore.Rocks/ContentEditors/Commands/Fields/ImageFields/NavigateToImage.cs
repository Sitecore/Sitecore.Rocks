// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Fields;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.ImageFields
{
    [ToolbarElement(typeof(ContentEditorFieldContext), 5200, "Image", "Navigate", Text = "Image", ElementType = RibbonElementType.SmallButton), Command(Submenu = "NavigateField")]
    public class NavigateToImage : CommandBase, IToolbarElement
    {
        public NavigateToImage()
        {
            Text = Resources.NavigateToImage_NavigateToImage_Image;
            Group = "Image";
            SortingValue = 100;
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

            var imageField = (ImageField)field.Control;

            var mediaUri = imageField.MediaUri;
            if (mediaUri == ItemUri.Empty)
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
            if (!(field.Control is ImageField))
            {
                return;
            }

            var imageField = (ImageField)field.Control;

            var mediaUri = imageField.MediaUri;
            if (mediaUri == ItemUri.Empty)
            {
                return;
            }

            AppHost.OpenContentEditor(new ItemVersionUri(mediaUri, LanguageManager.CurrentLanguage, Version.Empty));
        }
    }
}
