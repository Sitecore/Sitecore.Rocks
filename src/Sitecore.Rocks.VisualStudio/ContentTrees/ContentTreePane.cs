// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees
{
    [Guid(@"cbda4d53-d53a-4ad6-8069-2a680bc178b2")]
    public class ContentTreePane : ToolWindowPane, IPane, IServicePane, IActivatable
    {
        private ContentTree _contentTree;

        public ContentTreePane() : base(null)
        {
            Caption = Resources.SitecoreExplorer;
        }

        public void Activate()
        {
            var windowFrame = (IVsWindowFrame)Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        public object GetVsService(Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            return GetService(type);
        }

        protected override void Initialize()
        {
            base.Initialize();

            _contentTree = new ContentTree
            {
                Pane = this
            };

            Content = _contentTree;
            _contentTree.GotFocus += delegate { SitecorePackage.Instance.GotFocus(this); };
        }

        protected override void OnClose()
        {
            Notifications.RaiseUnloaded(this, _contentTree);
            base.OnClose();
        }
    }
}
