// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Commands.Tasks;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Dialogs.SelectTemplatesDialogs;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Tasks
{
    [Command(Submenu = TasksSubmenu.Name), CommandId(CommandIds.ItemEditor.SetBaseTemplates, typeof(ContentEditorContext))]

    // [ToolbarElement(typeof(ContentEditorContext), 2120, "Template", "Design", Text = "Base Templates", Icon = "Resources/32x32/Plus.png", ElementType = ToolbarElementType.SmallButton)]
    public class SetBaseTemplates : CommandBase, IToolbarElement
    {
        public SetBaseTemplates()
        {
            Text = Resources.SetBaseTemplates_SetBaseTemplates_Set_Base_Templates;
            Group = "Fields";
            SortingValue = 100;
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

            var field = GetBaseTemplatesField(context);
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

            var field = GetBaseTemplatesField(context);
            if (field == null)
            {
                return;
            }

            var value = field.Value;

            var selectedItems = new List<ItemId>();
            foreach (var s in value.Split('|'))
            {
                if (string.IsNullOrEmpty(s))
                {
                    continue;
                }

                selectedItems.Add(new ItemId(new Guid(s)));
            }

            var d = new SelectTemplatesDialog();
            d.Initialize(Resources.SetBaseTemplates_Execute_Base_Templates, field.FieldUris.First().ItemVersionUri.DatabaseUri, selectedItems);
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            value = string.Empty;
            foreach (var selectedItem in d.SelectedTemplates)
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

        [CanBeNull]
        private Field GetBaseTemplatesField([NotNull] ContentEditorContext context)
        {
            Debug.ArgumentNotNull(context, nameof(context));

            var baseTemplateId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Template/Data/__Base template");

            foreach (var field in context.ContentEditor.ContentModel.Fields)
            {
                if (field.FieldUris.First().FieldId == baseTemplateId)
                {
                    return field;
                }
            }

            return null;
        }
    }
}
