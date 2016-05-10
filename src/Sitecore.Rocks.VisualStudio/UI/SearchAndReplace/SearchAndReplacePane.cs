// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.UI.SearchAndReplace
{
    [Guid(@"5486bce8-7888-47ab-95d4-c8f5965d6fe1")]
    public class SearchAndReplacePane : ToolWindowPane, IPane
    {
        public SearchAndReplacePane() : base(null)
        {
            Caption = "Search and Replace";
            BitmapResourceID = 301;
            BitmapIndex = 1;
        }

        [NotNull]
        public SearchAndReplacePanel SearchAndReplace { get; private set; }

        public void Initialize([NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            var panel = (SearchAndReplacePanel)Content;

            panel.Initialize(databaseUri);
            panel.GotFocus += delegate { SitecorePackage.Instance.GotFocus(this); };
        }

        public void Initialize([NotNull] ItemUri itemUri)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            var panel = (SearchAndReplacePanel)Content;

            panel.Initialize(itemUri);
            panel.GotFocus += delegate { SitecorePackage.Instance.GotFocus(this); };
        }

        protected override void Initialize()
        {
            base.Initialize();

            SearchAndReplace = new SearchAndReplacePanel
            {
                Pane = this
            };

            Content = SearchAndReplace;
        }

        protected override void OnClose()
        {
            Notifications.RaiseUnloaded(this, SearchAndReplace);
            base.OnClose();
        }
    }
}
