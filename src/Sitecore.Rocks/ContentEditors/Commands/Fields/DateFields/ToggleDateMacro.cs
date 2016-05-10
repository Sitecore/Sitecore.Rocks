// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Fields;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.DateFields
{
    [Command]
    public class ToggleDateMacro : CommandBase
    {
        public ToggleDateMacro()
        {
            Text = "Date Macro ($date)";
            Group = "Date";
            SortingValue = 100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return false;
            }

            var dateTimeField = context.Field.Control as DateField;
            if (dateTimeField == null)
            {
                return false;
            }

            IsChecked = dateTimeField.IsDateMacro;

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return;
            }

            var dateTimeField = context.Field.Control as DateField;
            if (dateTimeField == null)
            {
                return;
            }

            dateTimeField.IsDateMacro = !dateTimeField.IsDateMacro;
        }
    }
}
