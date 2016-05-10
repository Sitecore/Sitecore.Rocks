// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentEditors.Commands.Navigating
{
    [Command(Submenu = "NavigateField"), Feature(FeatureNames.AdvancedNavigation)]
    public class NavigateToFieldTemplateItem : CommandBase
    {
        public NavigateToFieldTemplateItem()
        {
            Text = Resources.Navigate_to_Template_Field_Item;
            Group = "Navigate";
            SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return false;
            }

            if (context.Field.TemplateFieldId == ItemId.Empty)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return;
            }

            var field = context.Field.FieldUris.First();

            var uri = new ItemVersionUri(new ItemUri(field.ItemVersionUri.ItemUri.DatabaseUri, context.Field.TemplateFieldId), field.Language, Version.Latest);

            AppHost.OpenContentEditor(uri);
        }
    }
}
