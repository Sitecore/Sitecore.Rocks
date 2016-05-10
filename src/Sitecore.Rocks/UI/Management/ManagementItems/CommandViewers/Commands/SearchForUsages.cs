// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Management.ManagementItems.CommandViewers.Commands
{
    [Command]
    public class SearchForUsages : CommandBase
    {
        public SearchForUsages()
        {
            Text = Resources.SearchForUsages_SearchForUsages_Search_for_Usages;
            Group = "Browser";
            SortingValue = 1100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as CommandViewerContext;
            if (context == null)
            {
                return false;
            }

            var selectedItem = context.CommandViewer.CommandsList.SelectedItem as CommandViewer.CommandDescriptor;
            if (selectedItem == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as CommandViewerContext;
            if (context == null)
            {
                return;
            }

            var selectedItem = context.CommandViewer.CommandsList.SelectedItem as CommandViewer.CommandDescriptor;
            if (selectedItem == null)
            {
                return;
            }

            var databaseUri = new DatabaseUri(context.CommandViewer.Context.Site, new DatabaseName("core"));

            var queryAnalyzer = AppHost.Windows.Factory.OpenQueryAnalyzer(databaseUri);
            if (queryAnalyzer == null)
            {
                Trace.TraceError("Could not open query analyzer");
                return;
            }

            var script = string.Format("use core;\nselect * from //*[@Click='{0}' or @Message='{0}'];\nuse master;\nselect * from //*[@Click='{0}' or @Message='{0}']", selectedItem.Name);

            queryAnalyzer.SetScript(script);
            queryAnalyzer.Execute();
        }
    }
}
