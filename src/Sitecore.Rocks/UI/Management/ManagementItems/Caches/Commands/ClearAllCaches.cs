// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Caches.Commands
{
    [Command]
    public class ClearAllCaches : CommandBase
    {
        public ClearAllCaches()
        {
            Text = Resources.ClearAllCaches_ClearAllCaches_Clear_All___;
            Group = "Clear";
            SortingValue = 1500;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as CacheViewerContext;
            if (context == null)
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

            if (AppHost.MessageBox("Are you sure you want to clear all the caches?", "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                context.CacheViewer.LoadCaches();
            };

            context.CacheViewer.Context.Site.DataService.ExecuteAsync("Caches.ClearAll", completed);
        }
    }
}
