// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Management.ManagementItems.History.Commands
{
    [Command]
    public class Refresh : CommandBase
    {
        public Refresh()
        {
            Text = Resources.Refresh;
            Group = "Refresh";
            SortingValue = 9999;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as HistoryViewerContext;
            if (context == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as HistoryViewerContext;
            if (context == null)
            {
                return;
            }

            context.HistoryViewer.Refresh();
        }
    }
}
