// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.UI.XpathBuilder
{
    [Guid(@"811dd363-4fa2-42e6-a367-0645578de653")]
    public class XpathBuilderToolPane : ToolWindowPane, IEditorPane
    {
        private XpathBuilder xpathBuilder;

        public XpathBuilderToolPane() : base(null)
        {
            Caption = "XPath Builder";
            BitmapResourceID = 301;
            BitmapIndex = 1;
        }

        [NotNull]
        public override object Content
        {
            get
            {
                if (xpathBuilder != null)
                {
                    return xpathBuilder;
                }

                xpathBuilder = new XpathBuilder
                {
                    Pane = this
                };

                return xpathBuilder;
            }
        }

        public void Close()
        {
        }

        public void Initialize([NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            var builder = (XpathBuilder)Content;
            builder.Initialize(databaseUri);
            builder.GotFocus += delegate { SitecorePackage.Instance.GotFocus(this); };
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
