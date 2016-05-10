// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.UI.Management.ManagementItems.PipelineViewers.Commands
{
    [Command]
    public class Refresh : CommandBase
    {
        public Refresh()
        {
            Text = Resources.Refresh;
            Group = "Refresh";
            SortingValue = 9000;
            Icon = new Icon("Resources/16x16/refresh.png");
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is PipelineViewerContext;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as PipelineViewerContext;
            if (context == null)
            {
                return;
            }

            context.PipelineViewer.Refresh();
        }
    }
}
