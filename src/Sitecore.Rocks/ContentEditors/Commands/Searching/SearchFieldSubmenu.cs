// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.ContentEditors.Commands.Searching
{
    [Command]
    public class SearchFieldSubmenu : Submenu
    {
        public SearchFieldSubmenu()
        {
            Text = Resources.SearchSubmenu_SearchSubmenu_Search;
            Group = "Navigate";
            SortingValue = 5100;
            SubmenuName = "SearchField";
            ContextType = typeof(ContentEditorFieldContext);
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

            return base.CanExecute(parameter);
        }
    }
}
