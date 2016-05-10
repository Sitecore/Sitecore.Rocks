// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Fields;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.TextFields
{
    public abstract class TextOperation : CommandBase
    {
        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return false;
            }

            // TODO: test for field type control, not actual control
            return context.Field.Control is ISupportsTextOperations;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return;
            }

            var control = context.Field.Control;
            if (control == null)
            {
                return;
            }

            var value = control.GetValue();

            value = ProcessText(value);

            control.SetValue(value);
        }

        [NotNull]
        protected abstract string ProcessText([NotNull] string value);
    }
}
