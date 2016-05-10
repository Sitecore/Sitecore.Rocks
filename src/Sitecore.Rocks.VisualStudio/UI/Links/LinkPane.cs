// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.UI.Links
{
    [Guid(@"d6b15e81-fb21-40c6-a007-7f17e9ee4374")]
    public class LinkPane : ToolWindowPane, IEditorPane
    {
        public LinkPane() : base(null)
        {
            Caption = Resources.LinkPane_LinkPane_Links;
            BitmapResourceID = 301;
            BitmapIndex = 1;
        }

        [NotNull]
        public LinkViewer LinkViewer { get; private set; }

        public void Close()
        {
            var windowFrame = (IVsWindowFrame)Frame;

            ErrorHandler.ThrowOnFailure(windowFrame.Hide());
        }

        public void SetModifiedFlag(bool flag)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            LinkViewer = new LinkViewer
            {
                Pane = this
            };

            Content = LinkViewer;
            LinkViewer.GotFocus += delegate { SitecorePackage.Instance.GotFocus(this); };
        }

        protected override void OnClose()
        {
            Notifications.RaiseUnloaded(this, LinkViewer);
            base.OnClose();
        }
    }
}
