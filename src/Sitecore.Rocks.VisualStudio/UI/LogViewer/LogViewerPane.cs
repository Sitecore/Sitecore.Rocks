// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.UI.LogViewer
{
    [Guid(@"3d5a177d-cc4d-47e1-b0e7-97348700cfdf")]
    public class LogViewerPane : ToolWindowPane, IPane
    {
        public LogViewerPane() : base(null)
        {
            Caption = Resources.LogViewerPane_LogViewerPane_Log_Viewer;
            BitmapResourceID = 301;
            BitmapIndex = 1;
        }

        [NotNull]
        public LogViewer LogViewer { get; private set; }

        protected override void Initialize()
        {
            base.Initialize();

            LogViewer = new LogViewer
            {
                Pane = this
            };

            Content = LogViewer;
            LogViewer.GotFocus += delegate { SitecorePackage.Instance.GotFocus(this); };
        }

        protected override void OnClose()
        {
            Notifications.RaiseUnloaded(this, LogViewer);
            base.OnClose();
        }
    }
}
