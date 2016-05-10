// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.TemplateDesigner.Commands
{
    [Command, ToolbarElement(typeof(TemplateDesignerContext), 1510, "Home", "Template", ElementType = RibbonElementType.LargeButton, Icon = "Resources/32x32/Document-Text.png", Text = "Standard Value")]
    public class CreateStandardValues : CommandBase, IToolbarElement
    {
        public CreateStandardValues()
        {
            Text = Resources.CreateStandardValues_CreateStandardValues_Create_Standard_Values;
            Group = "Tasks";
            SortingValue = 90;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as TemplateDesignerContext;
            if (context == null)
            {
                return false;
            }

            return context.TemplateDesigner.StandardValueItemId == ItemId.Empty;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as TemplateDesignerContext;
            if (context == null)
            {
                return;
            }

            var parentUri = context.TemplateDesigner.TemplateUri;

            // create __Standard Values item
            var templateUri = context.TemplateDesigner.TemplateUri;

            var standardValuesItemUri = parentUri.Site.DataService.AddFromTemplate(parentUri, templateUri, @"__Standard Values");
            if (standardValuesItemUri == ItemUri.Empty)
            {
                return;
            }

            context.TemplateDesigner.StandardValueItemId = standardValuesItemUri.ItemId;

            // set "Standard Values" field
            var standardValuesField = new Field
            {
                Value = standardValuesItemUri.ItemId.ToString(),
                HasValue = true
            };

            standardValuesField.FieldUris.Add(new FieldUri(new ItemVersionUri(templateUri, LanguageManager.CurrentLanguage, Version.Latest), FieldIds.StandardValues));

            var fields = new List<Field>
            {
                standardValuesField
            };

            ItemModifier.Edit(standardValuesItemUri.DatabaseUri, fields, true);

            // expand tree
            var itemVersionUri = new ItemVersionUri(standardValuesItemUri, LanguageManager.CurrentLanguage, Version.Latest);

            AppHost.OpenContentEditor(itemVersionUri);

            Notifications.RaiseItemAdded(this, itemVersionUri, parentUri);
        }
    }
}
