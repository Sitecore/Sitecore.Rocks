// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Dialogs.SelectTemplatesDialogs;

namespace Sitecore.Rocks.ContentTrees.Commands.Templates
{
    [Command]
    public class SetBaseTemplates : CommandBase
    {
        public SetBaseTemplates()
        {
            Text = Resources.SetBaseTemplates_SetBaseTemplates_Set_Base_Templates;
            Group = "Templates";
            SortingValue = 100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return false;
            }

            var item = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (item == null)
            {
                return false;
            }

            return item.IsTemplate;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return;
            }

            var itemTreeViewItem = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (itemTreeViewItem == null)
            {
                return;
            }

            var fieldId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Template/Data/__Base template");

            GetValueCompleted<Item> completed = delegate(Item item)
            {
                var field = item.Fields.FirstOrDefault(f => f.Name == "__Base template");
                var selectedItems = field != null ? field.Value.Split('|').Where(v => !string.IsNullOrWhiteSpace(v)).Select(id => new ItemId(new Guid(id))) : Enumerable.Empty<ItemId>();

                var d = new SelectTemplatesDialog
                {
                    HelpText = "Each data template inherits from zero or more base data templates, which in turn specify base templates.",
                    Label = "Select the Base Templates:"
                };

                d.Initialize(Resources.SetBaseTemplates_Execute_Base_Templates, item.ItemUri.DatabaseUri, selectedItems);

                if (AppHost.Shell.ShowDialog(d) != true)
                {
                    return;
                }

                var baseTemplates = string.Join("|", d.SelectedTemplates.Select(i => i.ToString()));

                ItemModifier.Edit(item.ItemUri, fieldId, baseTemplates);
            };

            itemTreeViewItem.ItemUri.Site.DataService.GetItemFieldsAsync(new ItemVersionUri(itemTreeViewItem.ItemUri, Language.Current, Data.Version.Latest), completed);
        }
    }
}
