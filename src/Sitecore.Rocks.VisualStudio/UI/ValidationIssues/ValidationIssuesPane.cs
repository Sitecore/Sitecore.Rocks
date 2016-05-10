// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.UI.ValidationIssues
{
    [Guid(@"844fba0d-2dd6-42e8-aef8-d81675ef828e")]
    public class ValidationIssuesPane : ToolWindowPane, IPane
    {
        private ValidationIssues validationIssues;

        public ValidationIssuesPane() : base(null)
        {
            Caption = "Validation Issues";
            BitmapResourceID = 301;
            BitmapIndex = 1;
        }

        public void SetSource([NotNull] ItemUri itemUri)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            validationIssues.SetSource(itemUri);
        }

        protected override void Initialize()
        {
            base.Initialize();

            validationIssues = new ValidationIssues();

            Content = validationIssues;
            validationIssues.GotFocus += delegate { SitecorePackage.Instance.GotFocus(this); };
        }

        protected override void OnClose()
        {
            Notifications.RaiseUnloaded(this, Content);
            base.OnClose();
        }
    }
}
