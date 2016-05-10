// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.UI.Management
{
    [Guid(@"0aacb0dc-e025-4de6-a98d-5b0ef3836aa0")]
    public class ManagementViewerPane : ToolWindowPane, IPane
    {
        public ManagementViewerPane() : base(null)
        {
            Caption = "Management";
            BitmapResourceID = 301;
            BitmapIndex = 1;
        }

        [NotNull]
        public ManagementViewer ManagementViewer { get; private set; }

        protected override void Initialize()
        {
            base.Initialize();

            ManagementViewer = new ManagementViewer
            {
                Pane = this
            };

            Content = ManagementViewer;
            ManagementViewer.GotFocus += delegate { SitecorePackage.Instance.GotFocus(this); };
        }

        protected override void OnClose()
        {
            Notifications.RaiseUnloaded(this, ManagementViewer);

            base.OnClose();
        }
    }
}
