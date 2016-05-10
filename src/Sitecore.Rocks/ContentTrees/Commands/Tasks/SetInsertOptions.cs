// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Dialogs.SelectTemplatesDialogs;

namespace Sitecore.Rocks.ContentTrees.Commands.Tasks
{
    [Command(Submenu = TasksSubmenu.Name), CommandId(CommandIds.ItemEditor.SetInsertOptions, typeof(ContentTreeContext))]
    public class SetInsertOptions : CommandBase
    {
        public SetInsertOptions()
        {
            Text = Resources.SetInsertOptions_SetInsertOptions_Set_Insert_Options;
            Group = "Fields";
            SortingValue = 2900;
        }

        public override bool CanExecute(object parameter)
        {
            if (parameter is ContentEditorContext)
            {
                return false;
            }

            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (!context.Items.Any())
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var item = context.Items.FirstOrDefault();
            if (item == null)
            {
                return;
            }

            var selectedItems = new List<ItemId>();

            if (context.Items.Count() == 1)
            {
                if (!GetSelectedInsertOptions(item, selectedItems))
                {
                    return;
                }
            }

            var d = new SelectTemplatesDialog();
            d.Initialize(Resources.SetInsertOptions_Execute_Insert_Options, item.ItemUri.DatabaseUri, selectedItems, true);
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            var value = string.Empty;
            foreach (var selectedItem in d.SelectedTemplates)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    value += '|';
                }

                value += selectedItem.ToString();
            }

            var fieldId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Insert Options/Insert Options/__Masters");

            var fields = new List<Tuple<FieldUri, string>>();

            foreach (var i in context.Items)
            {
                var fieldUri = new FieldUri(new ItemVersionUri(i.ItemUri, LanguageManager.CurrentLanguage, Data.Version.Latest), fieldId);
                fields.Add(new Tuple<FieldUri, string>(fieldUri, value));
            }

            AppHost.Server.UpdateItems(fields);
        }

        private static bool GetSelectedInsertOptions([NotNull] IItem item, [NotNull] List<ItemId> selectedItems)
        {
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(selectedItems, nameof(selectedItems));

            Item i = null;
            bool[] busy =
            {
                true
            };

            GetValueCompleted<Item> completed = delegate(Item value)
            {
                if (!busy[0])
                {
                    return;
                }

                i = value;
                busy[0] = false;
            };

            AppHost.Server.GetItem(new ItemVersionUri(item.ItemUri, LanguageManager.CurrentLanguage, Data.Version.Latest), completed);

            if (!AppHost.DoEvents(ref busy[0]))
            {
                return false;
            }

            if (i == null)
            {
                return false;
            }

            var fieldId = IdManager.GetItemId("/sitecore/templates/System/Templates/Sections/Insert Options/Insert Options/__Masters");
            var field = i.Fields.FirstOrDefault(f => f != null && f.TemplateFieldId == fieldId);
            if (field == null || string.IsNullOrEmpty(field.Value))
            {
                return true;
            }

            var values = field.Value.Replace(",", "|");
            foreach (var s in values.Split('|'))
            {
                if (!string.IsNullOrEmpty(s))
                {
                    selectedItems.Add(new ItemId(new Guid(s)));
                }
            }

            return true;
        }
    }
}
