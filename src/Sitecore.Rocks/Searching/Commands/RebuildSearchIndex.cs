// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using TaskDialogInterop;

namespace Sitecore.Rocks.Searching.Commands
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
            var context = parameter as SearchContext;
            if (context == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as SearchContext;
            if (context == null)
            {
                return;
            }

            var options = new TaskDialogOptions
            {
                Title = "Rebuild Search Index",
                CommonButtons = TaskDialogCommonButtons.None,
                MainInstruction = "Are you sure you want to rebuild the Search Index?",
                Content = "Rebuilding the Search Index may take some time depending on the number of items in the database.",
                MainIcon = VistaTaskDialogIcon.Information,
                CommandButtons = new[]
                {
                    "Rebuild Search Index",
                    "Rebuild Search Index and show Job Viewer"
                },
                AllowDialogCancellation = true
            };

            var result = TaskDialog.Show(options);
            if (result.CommandButtonResult == null)
            {
                return;
            }

            AppHost.Server.RebuildSearchIndex(context.SearchViewer.DatabaseUri, AppHost.Server.HandleCompleted);

            if (result.CommandButtonResult == 1)
            {
                AppHost.Windows.OpenJobViewer(context.SearchViewer.DatabaseUri.Site);
            }
        }
    }
}
