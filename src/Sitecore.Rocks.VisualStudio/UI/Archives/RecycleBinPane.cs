// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.UI.Archives
{
    [Guid(@"10eb9b89-fa85-4189-a543-922f2a606676")]
    public class RecycleBinPane : ToolWindowPane, IPane
    {
        public RecycleBinPane() : base(null)
        {
            Caption = Resources.RecycleBinPane_RecycleBinPane_Recycle_Bin;
            BitmapResourceID = 301;
            BitmapIndex = 1;
        }

        [NotNull]
        public ArchiveViewer RecycleBinViewer { get; set; }

        public void Initialize([NotNull] DatabaseUri databaseUri, [NotNull] string caption)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(caption, nameof(caption));

            RecycleBinViewer.Initialize("recyclebin", Resources.RecycleBinPane_RecycleBinPane_Recycle_Bin, databaseUri);
        }

        protected override void Initialize()
        {
            base.Initialize();

            RecycleBinViewer = new ArchiveViewer
            {
                Pane = this
            };

            Content = RecycleBinViewer;
            RecycleBinViewer.GotFocus += delegate { SitecorePackage.Instance.GotFocus(this); };
        }

        protected override void OnClose()
        {
            Notifications.RaiseUnloaded(this, RecycleBinViewer);
            base.OnClose();
        }
    }
}
