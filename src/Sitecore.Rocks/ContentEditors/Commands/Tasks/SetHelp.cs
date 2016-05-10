// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Dialogs;
using Sitecore.Rocks.ContentTrees.Commands.Tasks;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentEditors.Commands.Tasks
{
    [Command(Submenu = TasksSubmenu.Name), CommandId(CommandIds.ItemEditor.SetHelp, typeof(ContentEditorContext))]
    public class SetHelp : CommandBase
    {
        public SetHelp()
        {
            Text = Resources.SetHelp_SetHelp_Set_Help;
            Group = "Fields";
            SortingValue = 4000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorContext;
            if (context == null)
            {
                return false;
            }

            if (!context.ContentEditor.ContentModel.IsSingle)
            {
                return false;
            }

            var field = GetHelpField(context, "__Long description");
            if (field == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentEditorContext;
            if (context == null)
            {
                return;
            }

            context.ContentEditor.ContentModel.GetChanges();

            var shortHelp = GetSelectedItems(context, "__Short description");
            var longHelp = GetSelectedItems(context, "__Long description");

            var d = new SetHelpDialog();

            d.Initialize(shortHelp, longHelp);
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            SetSelectedItems(context, "__Short description", d.ShortHelp.Text);
            SetSelectedItems(context, "__Long description", d.LongHelp.Text);

            context.ContentEditor.ContentModel.IsModified = true;
        }

        [CanBeNull]
        private Field GetHelpField([NotNull] ContentEditorContext context, [Localizable(false), NotNull] string fieldName)
        {
            Debug.ArgumentNotNull(context, nameof(context));
            Debug.ArgumentNotNull(fieldName, nameof(fieldName));

            var fieldId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Help/Help/" + fieldName);

            foreach (var field in context.ContentEditor.ContentModel.Fields)
            {
                if (field.FieldUris.First().FieldId == fieldId)
                {
                    return field;
                }
            }

            return null;
        }

        [NotNull]
        private string GetSelectedItems([NotNull] ContentEditorContext context, [Localizable(false), NotNull] string fieldName)
        {
            Debug.ArgumentNotNull(context, nameof(context));
            Debug.ArgumentNotNull(fieldName, nameof(fieldName));

            var field = GetHelpField(context, fieldName);
            if (field == null)
            {
                return string.Empty;
            }

            return field.Value;
        }

        private void SetSelectedItems([NotNull] ContentEditorContext context, [Localizable(false), NotNull] string fieldName, [NotNull] string value)
        {
            Debug.ArgumentNotNull(context, nameof(context));
            Debug.ArgumentNotNull(fieldName, nameof(fieldName));
            Debug.ArgumentNotNull(value, nameof(value));

            var field = GetHelpField(context, fieldName);
            if (field == null)
            {
                return;
            }

            field.Value = value;
            if (field.Control != null)
            {
                field.Control.SetValue(value);
            }
        }
    }
}
