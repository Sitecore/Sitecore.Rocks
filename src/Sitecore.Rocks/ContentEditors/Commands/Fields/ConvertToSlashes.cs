// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields
{
    [Command]
    public class ConvertToSlashes : CommandBase
    {
        public ConvertToSlashes()
        {
            Text = "Convert Backslashes to Slashes";
            Group = "Convert";
            SortingValue = 720;
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

            var fieldControl = field.Control;
            if (fieldControl == null)
            {
                return false;
            }

            var value = fieldControl.GetValue();
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            return value.Contains(@"\");
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

            fieldControl.SetValue(fieldControl.GetValue().Replace(@"\", @"/"));
        }
    }
}
