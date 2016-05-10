// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Templates
{
    [Command, CommandId(CommandIds.SitecoreExplorer.CreateStandardValues, typeof(ContentTreeContext))]
    public class CreateStandardValues : CommandBase
    {
        public CreateStandardValues()
        {
            Text = Resources.CreateStandardValues_CreateStandardValues_Create_Standard_Values;
            Group = "Templates";
            SortingValue = 50;
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

            if (!item.IsTemplate)
            {
                return false;
            }

            if (item.Item.StandardValuesField != ItemId.Empty)
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

            var item = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (item == null)
            {
                return;
            }

            var parentUri = item.ItemUri;

            // create __Standard Values item
            var templateUri = item.ItemUri;

            var standardValuesItemUri = parentUri.Site.DataService.AddFromTemplate(parentUri, templateUri, @"__Standard Values");
            if (standardValuesItemUri == ItemUri.Empty)
            {
                return;
            }

            // set "Standard Values" field
            var standardValuesFieldId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Advanced/Advanced/__Standard values");

            var standardValuesField = new Field
            {
                Value = standardValuesItemUri.ItemId.ToString(),
                HasValue = true
            };

            standardValuesField.FieldUris.Add(new FieldUri(new ItemVersionUri(item.ItemUri, LanguageManager.CurrentLanguage, Version.Latest), standardValuesFieldId));

            var fields = new List<Field>
            {
                standardValuesField
            };

            ItemModifier.Edit(standardValuesItemUri.DatabaseUri, fields, true);

            // expand tree
            context.ContentTree.ExpandTo(standardValuesItemUri);

            var itemVersionUri = new ItemVersionUri(standardValuesItemUri, LanguageManager.CurrentLanguage, Version.Latest);

            item.Item.StandardValuesId = standardValuesItemUri.ItemId;
            item.Item.StandardValuesField = standardValuesItemUri.ItemId;

            AppHost.OpenContentEditor(itemVersionUri);

            Notifications.RaiseItemAdded(this, itemVersionUri, parentUri);
        }
    }
}
