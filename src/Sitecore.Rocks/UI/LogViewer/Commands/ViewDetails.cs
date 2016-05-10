// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LogViewer.Commands
{
    [Command]
    public class ViewDetails : CommandBase
    {
        public ViewDetails()
        {
            Text = Resources.Details_Details_View_Details;
            Group = "Details";
            SortingValue = 100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LogViewerContext;
            if (context == null)
            {
                return false;
            }

            var selectedItems = context.LogViewer.ListView.SelectedItems;
            if (selectedItems == null)
            {
                return false;
            }

            if (selectedItems.Count != 1)
            {
                return false;
            }

            return true;
        }

        public override void Execute([CanBeNull] object parameter)
        {
            var context = parameter as LogViewerContext;
            if (context == null)
            {
                return;
            }

            var selectedItem = context.LogViewer.ListView.SelectedItem as LogItem;
            if (selectedItem == null)
            {
                return;
            }

            var d = new LogDetailsWindow();

            d.Initialize(selectedItem);

            AppHost.Shell.ShowDialog(d);
        }
    }
}
