// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentTrees.Commands.Editing
{
    /* [CommandId(CommandIds.SitecoreExplorer.EditItems, typeof(ContentTreeContext))] */

    [Command]
    public class EditItems : CommandBase
    {
        public EditItems()
        {
            Text = Resources.EditItems_EditItems_Edit;
            Group = "Edit";
            SortingValue = 1010;
            Icon = new Icon("Resources/16x16/pencil.png");
        }

        public override bool CanExecute(object parameter)
        {
            if (parameter is ContentEditorContext)
            {
                return false;
            }

            var selection = parameter as IItemSelectionContext;
            if (selection == null)
            {
                return false;
            }

            return selection.Items.Any();
        }

        public override void Execute(object parameter)
        {
            var selection = parameter as IItemSelectionContext;
            if (selection == null)
            {
                return;
            }

            var list = new List<ItemVersionUri>();

            foreach (var item in selection.Items)
            {
                list.Add(new ItemVersionUri(item.ItemUri, LanguageManager.CurrentLanguage, Version.Latest));
            }

            var options = new LoadItemsOptions(true)
            {
                NewTab = IsNewTab()
            };

            AppHost.Output.Log("Opening: " + string.Join(", ", list.Select(i => i.ItemId.ToString()).ToArray()));
            AppHost.Windows.Factory.OpenContentEditor(list, options);
        }

        protected virtual bool IsNewTab()
        {
            return (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
        }
    }
}
