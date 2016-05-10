// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.ContentTrees.Commands.Editing
{
    [Command]
    public class EditItemsNewTab : EditItems
    {
        public EditItemsNewTab()
        {
            Text = Resources.EditItemsNewTab_EditItemsNewTab_Edit_in_New_Tab;
            Group = "Edit";
            SortingValue = 1020;
            Icon = null;
        }

        public override bool CanExecute(object parameter)
        {
            if (!AppHost.Settings.Options.ReuseWindow)
            {
                return false;
            }

            return base.CanExecute(parameter);
        }

        protected override bool IsNewTab()
        {
            return true;
        }
    }
}
