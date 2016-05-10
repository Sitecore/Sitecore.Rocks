// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.UI.PublishingQueue
{
    [Guid(@"77e37355-f891-4b75-8443-4b7d4730737e")]
    public class PublishingQueuePane : ToolWindowPane, IPane
    {
        public PublishingQueuePane() : base(null)
        {
            Caption = "Publish Queue";
            BitmapResourceID = 301;
            BitmapIndex = 1;
        }

        [NotNull]
        public Publishing.PublishingQueue PublishingQueueViewer { get; private set; }

        public void Initialize([NotNull] DatabaseUri databaseUri, [NotNull] string caption)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(caption, nameof(caption));

            PublishingQueueViewer.Initialize(databaseUri);
        }

        protected override void Initialize()
        {
            base.Initialize();

            PublishingQueueViewer = new Publishing.PublishingQueue
            {
                Pane = this
            };

            Content = PublishingQueueViewer;
            PublishingQueueViewer.GotFocus += delegate { SitecorePackage.Instance.GotFocus(this); };
        }

        protected override void OnClose()
        {
            Notifications.RaiseUnloaded(this, PublishingQueueViewer);
            base.OnClose();
        }
    }
}
