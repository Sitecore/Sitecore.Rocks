// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Fields;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.ImageFields
{
    [ToolbarElement(typeof(ContentEditorFieldContext), 5210, "Image", "Navigate", Text = "Links", ElementType = RibbonElementType.SmallButton), Command]
    public class NavigateImageLinks : CommandBase, IToolbarElement
    {
        public NavigateImageLinks()
        {
            Text = Resources.NavigateImageLinks_NavigateImageLinks_Image_Links;
            Group = "Navigate";
            SortingValue = 5200;
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

            var linkViewer = AppHost.Windows.Factory.OpenLinkViewer();
            if (linkViewer == null)
            {
                return;
            }

            var linkTab = linkViewer.CreateTab(mediaUri);

            linkTab.Initialize(mediaUri);
        }
    }
}
