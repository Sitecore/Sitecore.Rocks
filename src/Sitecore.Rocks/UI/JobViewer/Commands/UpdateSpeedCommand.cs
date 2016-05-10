// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.JobViewer.Commands
{
    public abstract class UpdateSpeedCommand : CommandBase
    {
        public int UpdateSpeed { get; set; }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as JobViewerContext;
            if (context == null)
            {
                return false;
            }

            IsChecked = context.JobViewer.UpdateSpeed == UpdateSpeed;

            return true;
        }

        public override void Execute([CanBeNull] object parameter)
        {
            var context = parameter as JobViewerContext;
            if (context == null)
            {
                return;
            }

            context.JobViewer.UpdateSpeed = UpdateSpeed;

            context.JobViewer.Stop();

            AppHost.Settings.Set("Job Viewer", "Interval", UpdateSpeed);

            context.JobViewer.IsRunning = true;
            context.JobViewer.LoadJobs();
        }
    }
}
