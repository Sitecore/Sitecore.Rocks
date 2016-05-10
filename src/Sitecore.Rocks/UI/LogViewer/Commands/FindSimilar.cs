// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LogViewer.Commands
{
    [Command]
    public class FindSimilar : CommandBase
    {
        public FindSimilar()
        {
            Text = Resources.FindSimilar_FindSimilar_Find_Items_with_Same_Title;
            Group = "Search";
            SortingValue = 200;
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

            context.LogViewer.IncludeFilter.Text = selectedItem.Title;
        }
    }
}
