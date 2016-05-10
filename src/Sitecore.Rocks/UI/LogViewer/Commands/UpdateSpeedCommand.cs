// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LogViewer.Commands
{
    public abstract class UpdateSpeedCommand : CommandBase
    {
        public int UpdateSpeed { get; set; }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LogViewerContext;
            if (context == null)
            {
                return false;
            }

            IsChecked = context.LogViewer.UpdateSpeed == UpdateSpeed;

            return true;
        }

        public override void Execute([CanBeNull] object parameter)
        {
            var context = parameter as LogViewerContext;
            if (context == null)
            {
                return;
            }

            context.LogViewer.UpdateSpeed = UpdateSpeed;

            context.LogViewer.Stop();

            AppHost.Settings.Set("Log Viewer", "Interval", UpdateSpeed);

            context.LogViewer.IsRunning = true;
            context.LogViewer.LoadLog();
        }
    }
}
