// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.UI.JobViewer
{
    [Guid(@"7fc3540c-24da-4748-99ee-e62ed4559bfc")]
    public class JobViewerPane : ToolWindowPane, IPane
    {
        public JobViewerPane() : base(null)
        {
            Caption = Resources.JobViewerPane_JobViewerPane_Job_Viewer;
            BitmapResourceID = 301;
            BitmapIndex = 1;
        }

        [NotNull]
        public JobViewer JobViewer { get; private set; }

        protected override void Initialize()
        {
            base.Initialize();

            JobViewer = new JobViewer
            {
                Pane = this
            };

            Content = JobViewer;
            JobViewer.GotFocus += delegate { SitecorePackage.Instance.GotFocus(this); };
        }

        protected override void OnClose()
        {
            Notifications.RaiseUnloaded(this, JobViewer);
            base.OnClose();
        }
    }
}
