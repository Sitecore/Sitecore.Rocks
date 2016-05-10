// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Documents;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors.Dialogs;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.GuidExtensions;

namespace Sitecore.Rocks.UI.RuleEditors.Macros
{
    [RuleEditorMacro("template")]
    public class TemplateMacro : RuleEditorMacroBase
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

            var dialog = new ChangeTemplateDialog(DatabaseUri, ItemId.Empty, string.Empty);
            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            var template = dialog.SelectedTemplate;
            if (template == null)
            {
                return;
            }

            Value = template.TemplateUri.ItemId.ToString();
            run.Text = template.Name;
        }
    }
}
