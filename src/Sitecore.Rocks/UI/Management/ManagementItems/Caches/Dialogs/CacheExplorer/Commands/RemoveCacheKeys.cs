// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Text;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Caches.Dialogs.CacheExplorer.Commands
{
    [Command]
    public class RemoveCacheKeys : CommandBase
    {
        public RemoveCacheKeys()
        {
            Text = "Remove";
            Group = "Remove";
            SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as CacheExplorerContext;
            if (context == null)
            {
                return false;
            }

            var selectedItems = context.CacheExplorer.CacheKeyList.SelectedItems;
            if (selectedItems.Count == 0)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as CacheExplorerContext;
            if (context == null)
            {
                return;
            }

            var selectedItems = context.CacheExplorer.CacheKeyList.SelectedItems;

            var sb = new StringBuilder("|");

            foreach (var selectedItem in selectedItems)
            {
                var cacheKey = selectedItem as CacheExplorerDialog.CacheKeyDescriptor;
                if (cacheKey == null)
                {
                    continue;
                }

                sb.Append(cacheKey.Key);
                sb.Append('|');
            }

            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                context.CacheExplorer.Refresh();
            };

            context.CacheExplorer.Site.DataService.ExecuteAsync("Caches.RemoveCacheKeys", callback, context.CacheExplorer.CacheName, sb.ToString());
        }
    }
}
