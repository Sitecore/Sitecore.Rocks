// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.UI.Archives
{
    [Guid(@"0d53984b-f5d7-4dc4-8e74-4860ec6b1535")]
    public class ArchivePane : ToolWindowPane, IPane
    {
        public ArchivePane() : base(null)
        {
            Caption = Resources.ArchivePane_ArchivePane_Archive;
            BitmapResourceID = 301;
            BitmapIndex = 1;
        }

        [NotNull]
        public ArchiveViewer ArchiveViewer { get; private set; }

        public void Initialize([NotNull] DatabaseUri databaseUri, [NotNull] string caption)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(caption, nameof(caption));

            ArchiveViewer.Initialize("archive", Resources.ArchivePane_ArchivePane_Archive, databaseUri);
            ArchiveViewer.GotFocus += delegate { SitecorePackage.Instance.GotFocus(this); };
        }

        protected override void Initialize()
        {
            base.Initialize();

            ArchiveViewer = new ArchiveViewer
            {
                Pane = this
            };

            Content = ArchiveViewer;
        }

        protected override void OnClose()
        {
            Notifications.RaiseUnloaded(this, ArchiveViewer);
            base.OnClose();
        }
    }
}
