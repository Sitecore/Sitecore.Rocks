// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.Searching
{
    [Guid(@"30b196e4-f1d3-42af-8e05-c6f0c39bd9e8")]
    public class SearchPane : ToolWindowPane, IPane
    {
        public SearchPane() : base(null)
        {
            Caption = Resources.SearchPane_SearchPane_Search_in_Sitecore;
        }

        [NotNull]
        public SearchViewer SearchViewer { get; private set; }

        [NotNull]
        public override IWin32Window Window
        {
            get { return (IWin32Window)SearchViewer; }
        }

        protected override void Initialize()
        {
            base.Initialize();

            SearchViewer = new SearchViewer
            {
                Pane = this
            };

            Content = SearchViewer;
            SearchViewer.GotFocus += delegate { SitecorePackage.Instance.GotFocus(this); };
        }

        protected override void OnClose()
        {
            Notifications.RaiseUnloaded(this, SearchViewer);
            base.OnClose();
        }
    }
}
