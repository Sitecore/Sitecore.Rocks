// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields
{
    [Command, Feature(FeatureNames.AdvancedOperations)]
    public class HideInNonStandardFieldsView : CommandBase
    {
        public HideInNonStandardFieldsView()
        {
            Text = Resources.HideInNonStandardFieldsView_HideInNonStandardFieldsView_Hide_in_Non_Standard_Fields_View;
            Group = "Views";
            SortingValue = 9100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return false;
            }

            if (FieldManager.IsStandardField(context.Field))
            {
                return false;
            }

            IsChecked = !FieldManager.GetContentFieldVisibility(context.Field.TemplateFieldId);

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return;
            }

            FieldManager.SetContentFieldVisibility(context.Field.TemplateFieldId, IsChecked);

            context.ContentEditor.Refresh();
        }
    }
}
