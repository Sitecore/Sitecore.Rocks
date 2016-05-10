// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.Searching.Commands
{
    [Command]
    public class ResetFilters : CommandBase
    {
        public ResetFilters()
        {
            Text = Resources.ResetFilters_ResetFilters_Reset_Filters;
            Group = "Search";
            SortingValue = 7000;
        }

        public override bool CanExecute(object parameter)
        {
            return parameter as SearchContext != null;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as SearchContext;
            if (context == null)
            {
                return;
            }

            context.SearchViewer.ResetFilters();
        }
    }
}
