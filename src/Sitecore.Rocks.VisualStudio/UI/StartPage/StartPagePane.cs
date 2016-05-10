// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.UI.StartPage
{
    [Guid(@"cfc9a85c-e149-4443-8093-b99ff6bddde0")]
    public class StartPagePane : ToolWindowPane, IPane
    {
        public StartPagePane() : base(null)
        {
            Caption = "Sitecore Start Page";
        }

        [NotNull]
        public StartPageViewer StartPageViewer { get; private set; }

        [NotNull]
        public override IWin32Window Window
        {
            get { return (IWin32Window)StartPageViewer; }
        }

        protected override void Initialize()
        {
            base.Initialize();

            StartPageViewer = new StartPageViewer
            {
                Pane = this
            };

            Content = StartPageViewer;
            StartPageViewer.GotFocus += delegate { SitecorePackage.Instance.GotFocus(this); };
        }

        protected override void OnClose()
        {
            Notifications.RaiseUnloaded(this, StartPageViewer);
            base.OnClose();
        }
    }
}
