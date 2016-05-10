// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Publishing.Commands
{
    [Command]
    public class Filters : CommandBase
    {
        public Filters()
        {
            Text = Resources.Filters_Filters_Filters;
            Group = "Filters";
            SortingValue = 7000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as PublishingQueueContext;
            if (context == null)
            {
                return false;
            }

            IsChecked = context.PublishingQueueViewer.FilterHeight.Height.Value != 0;

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as PublishingQueueContext;
            if (context == null)
            {
                return;
            }

            context.PublishingQueueViewer.ToggleFilters();
        }
    }
}
