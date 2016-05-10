// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.Media.Commands
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
            return parameter as MediaContext != null;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as MediaContext;
            if (context == null)
            {
                return;
            }

            context.MediaViewer.ResetFilters();
        }
    }
}
