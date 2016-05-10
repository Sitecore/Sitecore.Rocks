// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.UI.QueryAnalyzer
{
    [Guid(@"8BF4030D-C2AC-4F02-BDCA-1B2AC69477AB")]
    public class QueryAnalyzerToolPane : ToolWindowPane, IEditorPane
    {
        private QueryAnalyzers.QueryAnalyzer queryAnalyzer;

        public QueryAnalyzerToolPane() : base(null)
        {
            Caption = Resources.ToolWindowTitle;
            BitmapResourceID = 301;
            BitmapIndex = 1;
        }

        [NotNull]
        public override object Content
        {
            get
            {
                if (queryAnalyzer != null)
                {
                    return queryAnalyzer;
                }

                queryAnalyzer = new QueryAnalyzers.QueryAnalyzer
                {
                    Pane = this
                };

                return queryAnalyzer;
            }
        }

        public void Close()
        {
        }

        public void Initialize([NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            var analyzer = (QueryAnalyzers.QueryAnalyzer)Content;

            analyzer.Initialize(databaseUri);
            analyzer.GotFocus += delegate { SitecorePackage.Instance.GotFocus(this); };
        }

        public void SetModifiedFlag(bool flag)
        {
        }

        protected override void OnClose()
        {
            Notifications.RaiseUnloaded(this, Content);
            base.OnClose();
        }
    }
}
