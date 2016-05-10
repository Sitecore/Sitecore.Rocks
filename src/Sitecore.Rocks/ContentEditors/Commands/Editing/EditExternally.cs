// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentEditors.Commands.Editing
{
    [Command, CommandId(CommandIds.ItemEditor.EditExternally, typeof(ContentEditorContext))]
    public class EditExternally : CommandBase
    {
        public EditExternally()
        {
            Text = Resources.Edit_Externally;
            Group = "Edit";
            SortingValue = 2510;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return false;
            }

            var field = context.Field;
            if (field.Control == null)
            {
                return false;
            }

            if (field.IsBlob)
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

            Action<string> edited = s => context.ContentEditor.Dispatcher.Invoke(new Action(() => fieldControl.SetValue(s)));

            FieldControlManager.AddWatcher(fieldControl, fieldControl.GetValue(), context.Field.Name + @".field", @"html", edited);
        }
    }
}
