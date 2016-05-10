// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields
{
    [Command, Feature(FeatureNames.AdvancedOperations)]
    public class ShowInNonStandardFieldsView : CommandBase
    {
        public ShowInNonStandardFieldsView()
        {
            Text = Resources.ShowInNonStandardFieldsView_ShowInNonStandardFieldsView_Show_in_Non_Standard_Fields_View;
            Group = "Views";
            SortingValue = 9000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return false;
            }

            if (!FieldManager.IsStandardField(context.Field))
            {
                return false;
            }

            IsChecked = FieldManager.GetStandardFieldVisibility(context.Field.TemplateFieldId);

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return;
            }

            FieldManager.SetStandardFieldVisibility(context.Field.TemplateFieldId, !IsChecked);

            context.ContentEditor.Refresh();
        }
    }
}
