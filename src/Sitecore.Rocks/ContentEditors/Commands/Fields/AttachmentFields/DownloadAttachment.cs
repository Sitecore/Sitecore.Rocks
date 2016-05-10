// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Windows.Forms;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Fields;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Media;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.AttachmentFields
{
    [Command]
    public class DownloadAttachment : CommandBase
    {
        public DownloadAttachment()
        {
            Text = Resources.DownloadAttachment_DownloadAttachment_Download;
            Group = "Attachment";
            SortingValue = 100;
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

            var value = fieldControl.GetValue();
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            Guid id;
            return Guid.TryParse(value, out id);
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return;
            }

            var name = context.ContentEditor.ContentModel.Items.First().Name;
            var filter = string.Format(@"{0}|*.*", Resources.All_files);
            var extension = string.Empty;

            var extensionField = context.ContentEditor.ContentModel.Fields.FirstOrDefault(field => field.Name == @"Extension");

            if (extensionField != null)
            {
                extension = extensionField.Value;
            }

            if (string.IsNullOrEmpty(name))
            {
                name = @"File";
            }

            if (!string.IsNullOrEmpty(extension))
            {
                name = name + @"." + extension;
                filter = string.Format(@"{0} {3}|*.{1}|{2}", extension.Capitalize(), extension, filter, Resources.DownloadAttachment_Execute_files);
            }

            var dialog = new SaveFileDialog
            {
                Title = Resources.MediaManager_DownloadAttachment_Download,
                CheckPathExists = true,
                OverwritePrompt = true,
                FileName = name,
                Filter = filter
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            MediaManager.DownloadAttachment(context.Field.FieldUris.First().ItemVersionUri.ItemUri, dialog.FileName);
        }
    }
}
