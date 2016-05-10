// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Shell.Panes;

namespace Sitecore.Rocks.UI.TemplateDesigner
{
    [Guid(@"1390f623-4186-46b9-bf78-2e4c13488845")]
    public class TemplateDesignerPane : EditorPane<TemplateDesignerFactory, TemplateDesigner>, IEditorPane
    {
        private TemplateDesigner templateDesigner;

        public void Close()
        {
            var windowFrame = (IVsWindowFrame)Frame;

            ErrorHandler.ThrowOnFailure(windowFrame.Hide());
        }

        public void Initialize([NotNull] ItemUri templateUri)
        {
            Assert.ArgumentNotNull(templateUri, nameof(templateUri));

            templateDesigner.Initialize(templateUri);
        }

        public void SetModifiedFlag(bool flag)
        {
            if (flag)
            {
                SetModified();
            }
        }

        protected override void Initialize()
        {
            base.Initialize();

            templateDesigner = (TemplateDesigner)Content;
            templateDesigner.Pane = this;
            templateDesigner.GotFocus += delegate { SitecorePackage.Instance.GotFocus(this); };
        }

        protected override void OnClose()
        {
            Notifications.RaiseUnloaded(this, templateDesigner);
            base.OnClose();
        }

        protected override void SaveFile(string fileName)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));

            templateDesigner.Save();
        }
    }
}
