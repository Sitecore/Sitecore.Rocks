// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.Management.ManagementItems.Caches.Dialogs.CacheExplorer;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Caches.Commands
{
    [Command]
    public class CacheExplorer : CommandBase
    {
        public CacheExplorer()
        {
            Text = "Explore Cache...";
            Group = "Explore";
            SortingValue = 100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as CacheViewerContext;
            if (context == null)
            {
                return false;
            }

            var selectedItems = context.CacheViewer.CacheList.SelectedItems;
            if (selectedItems.Count != 1)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as CacheViewerContext;
            if (context == null)
            {
                return;
            }

            var cache = context.CacheViewer.CacheList.SelectedItem as CacheViewer.CacheDescriptor;
            if (cache == null)
            {
                return;
            }

            var dialog = new CacheExplorerDialog(context.CacheViewer.Context.Site, cache.Name);
            AppHost.Shell.ShowDialog(dialog);
        }
    }
}
