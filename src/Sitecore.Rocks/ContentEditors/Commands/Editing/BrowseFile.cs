// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.Dialogs.SelectFileDialogs;

namespace Sitecore.Rocks.ContentEditors.Commands.Editing
{
    [Command]
    public class BrowseFile : CommandBase
    {
        public BrowseFile()
        {
            Text = "Browse for File...";
            Group = "Edit";
            SortingValue = 510;
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

            if (field.Name != @"Path")
            {
                return false;
            }

            var control = context.Field.Control;
            if (control == null)
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

            var fieldUri = context.Field.FieldUris.FirstOrDefault();
            if (fieldUri == null)
            {
                return;
            }

            var dialog = new SelectFileDialog
            {
                Site = fieldUri.DatabaseUri.Site
            };

            if (!dialog.ShowDialog())
            {
                return;
            }

            var control = context.Field.Control;
            if (control != null)
            {
                control.SetValue(dialog.SelectedFilePath.Replace("\\", "/"));
            }
        }
    }
}
