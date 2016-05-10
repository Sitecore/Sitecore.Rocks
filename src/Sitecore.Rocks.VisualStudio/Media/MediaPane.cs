// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.Media
{
    [Guid(@"93876711-7815-4a5e-9fb9-7a8e2602630d")]
    public class MediaPane : ToolWindowPane, IPane
    {
        public MediaPane() : base(null)
        {
            Caption = Resources.MediaPane_MediaPane_Media;
        }

        public MediaViewer MediaViewer { get; private set; }

        [NotNull]
        public override IWin32Window Window
        {
            get { return (IWin32Window)MediaViewer; }
        }

        protected override void Initialize()
        {
            base.Initialize();

            MediaViewer = new MediaViewer
            {
                Pane = this
            };

            Content = MediaViewer;
            MediaViewer.GotFocus += delegate { SitecorePackage.Instance.GotFocus(this); };
        }

        protected override void OnClose()
        {
            Notifications.RaiseUnloaded(this, MediaViewer);
            base.OnClose();
        }
    }
}
