// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentEditors.Commands.Editing
{
    [Command]
    public class ClearField : CommandBase
    {
        public ClearField()
        {
            Text = Resources.Clear;
            Group = "Edit";
            SortingValue = 2000;
            Icon = new Icon("Resources/16x16/delete.png");
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

            if (field.Control == null)
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
            if (fieldControl != null)
            {
                fieldControl.SetValue(string.Empty);
            }
        }
    }
}
