// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.ContentEditors.Commands.Editing
{
    [Command]
    public class EditFile : CommandBase
    {
        public EditFile()
        {
            Text = Resources.EditFile_EditFile_Edit_File;
            Group = "Edit";
            SortingValue = 500;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return false;
            }

            var field = context.Field;
            if (field.IsBlob)
            {
                return false;
            }

            if (field.Name != @"Path" && field.Name != @"File")
            {
                return false;
            }

            var fileName = GetFileName(field);
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            return File.Exists(fileName);
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return;
            }

            var fileName = GetFileName(context.Field);
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            AppHost.Files.OpenFile(fileName);
        }

        [NotNull]
        public string GetFileName([NotNull] Field field)
        {
            Assert.ArgumentNotNull(field, nameof(field));

            var fieldControl = field.Control;
            if (fieldControl == null)
            {
                return string.Empty;
            }

            var fileName = fieldControl.GetValue();
            if (fileName.StartsWith(@"/") || fileName.StartsWith(@"\\"))
            {
                fileName = fileName.Mid(1);
            }

            var fieldUri = field.FieldUris.First();

            return AppHost.Projects.MakeAbsoluteFileName(fieldUri.Site, fileName) ?? string.Empty;
        }
    }
}
