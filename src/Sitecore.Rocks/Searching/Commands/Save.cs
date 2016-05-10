// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.Searching.Commands
{
    [Command]
    public class Save : CommandBase
    {
        public Save()
        {
            Text = Resources.Save_Save_Save_Search;
            Group = "Save";
            SortingValue = 6000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as SearchContext;
            if (context == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(context.SearchViewer.LastQueryText))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as SearchContext;
            if (context == null)
            {
                return;
            }

            var name = AppHost.Prompt(Resources.Save_Execute_Enter_a_name_that_identifies_the_search_, Resources.Save_Execute_Save_Search, context.SearchViewer.LastQueryText);
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            var savedSearch = new SavedSearch
            {
                Site = context.SearchViewer.Site,
                Name = name,
                QueryText = context.SearchViewer.LastQueryText,
                Field = context.SearchViewer.LastField
            };

            SearchManager.Add(savedSearch);
            SearchManager.Save();
        }
    }
}
