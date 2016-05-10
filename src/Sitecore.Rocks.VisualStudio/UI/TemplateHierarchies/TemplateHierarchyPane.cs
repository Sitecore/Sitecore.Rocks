// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.UI.TemplateHierarchies
{
    [Guid(@"d40cd3f5-e053-4018-bfc7-d3e20d874fa4")]
    public class TemplateHierarchyPane : ToolWindowPane, IEditorPane
    {
        public TemplateHierarchyPane() : base(null)
        {
            Caption = "Template Hierarchy";
            BitmapResourceID = 301;
            BitmapIndex = 1;
        }

        [NotNull]
        public TemplateHierarchyViewer TemplateHierarchyViewer { get; private set; }

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

            TemplateHierarchyViewer = new TemplateHierarchyViewer
            {
                Pane = this
            };

            Content = TemplateHierarchyViewer;
            TemplateHierarchyViewer.GotFocus += delegate { SitecorePackage.Instance.GotFocus(this); };
        }

        protected override void OnClose()
        {
            Notifications.RaiseUnloaded(this, TemplateHierarchyViewer);
            base.OnClose();
        }
    }
}
