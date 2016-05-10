// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Dialogs.SetValidators;
using Sitecore.Rocks.ContentTrees.Commands.Tasks;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentEditors.Commands.Tasks
{
    [Command(Submenu = TasksSubmenu.Name), CommandId(CommandIds.ItemEditor.SetValidators, typeof(ContentEditorContext))]
    public class SetValidators : CommandBase
    {
        public SetValidators()
        {
            Text = Resources.SetValidators_SetValidators_Set_Validation;
            Group = "Fields";
            SortingValue = 5000;
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

            if (!context.ContentEditor.ContentModel.FirstItem.ItemUri.Site.DataService.CanExecuteAsync("Validation.GetValidators"))
            {
                return false;
            }

            var field = GetValidatorField(context, "__Quick Action Bar Validation Rules");
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

            var quickActionBarValidators = GetSelectedItems(context, "__Quick Action Bar Validation Rules");
            var validateButtonValidators = GetSelectedItems(context, "__Validate Button Validation Rules");
            var validatorBarValidators = GetSelectedItems(context, "__Validator Bar Validation Rules");
            var workflowValidators = GetSelectedItems(context, "__Workflow Validation Rules");

            var d = new SelectValidatorsDialog();
            var databaseUri = context.ContentEditor.ContentModel.Fields.First().FieldUris.First().ItemVersionUri.DatabaseUri;

            d.Initialize(Resources.SetValidators_Execute_Validators, databaseUri, quickActionBarValidators, validateButtonValidators, validatorBarValidators, workflowValidators);
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            SetSelectedItems(context, "__Quick Action Bar Validation Rules", d.QuickActionBar.SelectedItems);
            SetSelectedItems(context, "__Validate Button Validation Rules", d.ValidateButton.SelectedItems);
            SetSelectedItems(context, "__Validator Bar Validation Rules", d.ValidatorBar.SelectedItems);
            SetSelectedItems(context, "__Workflow Validation Rules", d.Workflow.SelectedItems);
        }

        [NotNull]
        private List<ItemId> GetSelectedItems([NotNull] ContentEditorContext context, [Localizable(false), NotNull] string fieldName)
        {
            Debug.ArgumentNotNull(context, nameof(context));
            Debug.ArgumentNotNull(fieldName, nameof(fieldName));

            var selectedItems = new List<ItemId>();

            var field = GetValidatorField(context, fieldName);
            if (field == null)
            {
                return selectedItems;
            }

            var value = field.Value;

            foreach (var s in value.Split('|'))
            {
                if (string.IsNullOrEmpty(s))
                {
                    continue;
                }

                selectedItems.Add(new ItemId(new Guid(s)));
            }

            return selectedItems;
        }

        [CanBeNull]
        private Field GetValidatorField([NotNull] ContentEditorContext context, [Localizable(false), NotNull] string fieldName)
        {
            Debug.ArgumentNotNull(context, nameof(context));
            Debug.ArgumentNotNull(fieldName, nameof(fieldName));

            var fieldId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Validators/Validation Rules/" + fieldName);

            foreach (var field in context.ContentEditor.ContentModel.Fields)
            {
                if (field.FieldUris.First().FieldId == fieldId)
                {
                    return field;
                }
            }

            return null;
        }

        private void SetSelectedItems([NotNull] ContentEditorContext context, [Localizable(false), NotNull] string fieldName, [NotNull] List<ItemId> selectedItems)
        {
            Debug.ArgumentNotNull(context, nameof(context));
            Debug.ArgumentNotNull(fieldName, nameof(fieldName));
            Debug.ArgumentNotNull(selectedItems, nameof(selectedItems));

            var field = GetValidatorField(context, fieldName);
            if (field == null)
            {
                return;
            }

            var value = string.Empty;

            foreach (var selectedItem in selectedItems)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    value += '|';
                }

                value += selectedItem.ToString();
            }

            field.Value = value;
            if (field.Control != null)
            {
                field.Control.SetValue(value);
            }
        }
    }
}
