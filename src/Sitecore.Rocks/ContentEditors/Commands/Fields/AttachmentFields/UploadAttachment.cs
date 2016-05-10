// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Microsoft.Win32;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Fields;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Media;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.AttachmentFields
{
    [Command]
    public class UploadAttachment : CommandBase
    {
        public UploadAttachment()
        {
            Text = "Upload...";
            Group = "Attachment";
            SortingValue = 90;
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

            var attachment = fieldControl as Attachment;
            if (attachment == null)
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

            var attachment = fieldControl as Attachment;
            if (attachment == null)
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

            GetValueCompleted<bool> uploadCompleted = delegate { fieldControl.SetValue(fieldControl.GetValue()); };

            MediaManager.Attach(context.Field.FieldUris.First().ItemVersionUri.ItemUri, @"/upload/Images/Uploaded", dialog.FileName, uploadCompleted);
        }
    }
}
