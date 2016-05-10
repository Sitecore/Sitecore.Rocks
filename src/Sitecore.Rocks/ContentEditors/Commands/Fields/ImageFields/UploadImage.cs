// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Microsoft.Win32;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Fields;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Media;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.ImageFields
{
    [ToolbarElement(typeof(ContentEditorFieldContext), 2591, "Image", "Source", Text = "Upload", ElementType = RibbonElementType.LargeButton, Icon = "Resources/32x32/Upload.png"), Command]
    public class UploadImage : CommandBase, IToolbarElement
    {
        public UploadImage()
        {
            Text = "Upload Image...";
            Group = "Image";
            SortingValue = 15;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return false;
            }

            var fieldControl = context.Field.Control;
            if (fieldControl == null)
            {
                return false;
            }

            var imageField = fieldControl as ImageField;
            if (imageField == null)
            {
                return false;
            }

            if (context.Field.FieldUris.Count != 1)
            {
                return false;
            }

            if (!context.Field.FieldUris.First().Site.DataService.CanExecuteAsync("Media.Upload"))
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

            var fieldControl = context.Field.Control;
            if (fieldControl == null)
            {
                return;
            }

            var imageField = fieldControl as ImageField;
            if (imageField == null)
            {
                return;
            }

            var dialog = new OpenFileDialog
            {
                Title = "Upload Media",
                CheckFileExists = true,
                Filter = @"All files|*.*",
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            GetValueCompleted<ItemHeader> uploadCompleted = delegate(ItemHeader value) { imageField.Update(value); };

            MediaManager.Upload(context.Field.FieldUris.First().DatabaseUri, @"/sitecore/media library", dialog.FileName, uploadCompleted);
        }
    }
}
