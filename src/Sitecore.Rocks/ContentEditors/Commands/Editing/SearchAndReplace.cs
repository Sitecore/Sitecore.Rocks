// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.ContentEditors.Commands.Editing
{
    [Command]
    public class SearchAndReplace : CommandBase
    {
        public SearchAndReplace()
        {
            Text = Resources.SearchAndReplace_SearchAndReplace_Search_and_Replace;
            Group = "Edit";
            SortingValue = 2650;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return false;
            }

            var field = context.Field;
            if (field.IsBlob)
            {
                return false;
            }

            if (field.Control == null)
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

            var fieldControl = context.Field.Control;
            if (fieldControl == null)
            {
                return;
            }

            var panel = AppHost.Windows.Factory.OpenSearchAndReplace(context.Field.FieldUris.First().ItemVersionUri.DatabaseUri);
            if (panel == null)
            {
                return;
            }

            panel.FieldNameCombo.Text = context.Field.Name;
            panel.FindCombo.Text = fieldControl.GetValue();
        }
    }
}
