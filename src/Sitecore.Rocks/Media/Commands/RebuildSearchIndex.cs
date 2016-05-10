// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.Media.Commands
{
    [Command]
    public class RebuildSearchIndex : CommandBase
    {
        public RebuildSearchIndex()
        {
            Text = Resources.RebuildSearchIndex_Execute_Rebuild_Search_Index;
            Group = "Rebuild";
            SortingValue = 8000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as MediaContext;
            if (context == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as MediaContext;
            if (context == null)
            {
                return;
            }

            AppHost.Server.RebuildSearchIndex(context.MediaViewer.DatabaseUri, AppHost.Server.HandleCompleted);

            AppHost.MessageBox(Resources.SearchViewer_RebuildSearchIndex_Rebuild_Search_Index_started___, Resources.SearchViewer_RebuildSearchIndex_Rebuild_Search_Index, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
