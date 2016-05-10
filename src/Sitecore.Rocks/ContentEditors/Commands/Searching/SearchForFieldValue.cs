// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentEditors.Commands.Searching
{
    [Command(Submenu = "SearchField"), Feature(FeatureNames.AdvancedNavigation)]
    public class SearchForFieldValue : CommandBase
    {
        public SearchForFieldValue()
        {
            Text = Resources.SearchForFieldValue_SearchForFieldValue_Search_for_Field_Value_in_this_Field;
            Group = "Search";
            SortingValue = 2000;
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

            var fieldControl = field.Control;
            if (fieldControl == null)
            {
                return false;
            }

            var value = fieldControl.GetValue();
            if (string.IsNullOrEmpty(value))
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

            var value = fieldControl.GetValue();
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            var searchWindow = AppHost.Windows.Factory.OpenSearchViewer(context.Field.FieldUris.First().Site);
            if (searchWindow == null)
            {
                return;
            }

            searchWindow.Search(context.Field.Name, @"""" + value + @"""");
        }
    }
}
