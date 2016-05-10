// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Documents;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.GuidExtensions;

namespace Sitecore.Rocks.UI.RuleEditors.Macros
{
    [RuleEditorMacro("text")]
    public class TextMacro : RuleEditorMacroBase
    {
        private Hyperlink control;

        private Run run;

        public override object GetEditableControl()
        {
            var value = GetValue();

            run = new Run(value);
            control = new Hyperlink(run);
            control.Click += Edit;

            if (value.IsGuid())
            {
                GetItemName(value, s => run.Text = s);
            }

            return control;
        }

        private void Edit([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var result = AppHost.Prompt(Resources.TextMacro_Edit_Enter_a_new_value_, Resources.TextMacro_Edit_Value, Value);
            if (result == null)
            {
                return;
            }

            Value = result;
            run.Text = result;
        }
    }
}
