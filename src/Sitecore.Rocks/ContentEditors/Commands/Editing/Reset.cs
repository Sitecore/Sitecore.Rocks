// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.ContentEditors.Commands.Editing
{
    [Command]
    public class Reset : CommandBase
    {
        public Reset()
        {
            Text = Resources.Reset_to_Standard_Value_on_Save;
            Group = "Edit";
            SortingValue = 2500;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return false;
            }

            if (context.Field.StandardValue)
            {
                return false;
            }

            IsChecked = context.Field.ResetOnSave;

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
            field.ResetOnSave = !field.ResetOnSave;
            context.ContentEditor.ContentModel.IsModified = true;

            var fieldControl = field.Control;
            if (fieldControl == null)
            {
                return;
            }

            if (field.ResetOnSave)
            {
                fieldControl.SetValue(string.Empty);
            }

            var control = fieldControl.GetControl();
            control.IsEnabled = !field.ResetOnSave;
        }
    }
}
