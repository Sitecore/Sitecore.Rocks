// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Shell.Panes;

namespace Sitecore.Rocks.CodeGeneration
{
    [Guid(@"a498a11d-2c8c-486d-b591-b9999e0d9e80")]
    public class CodeGenerationPane : EditorPane<CodeGenerationFactory, CodeGenerationConfigurator>, IEditorPane
    {
        private CodeGenerationConfigurator configurator;

        public void Close()
        {
            var windowFrame = (IVsWindowFrame)Frame;

            ErrorHandler.ThrowOnFailure(windowFrame.Hide());
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

            configurator = (CodeGenerationConfigurator)Content;
            configurator.Pane = this;
            configurator.GotFocus += delegate { SitecorePackage.Instance.GotFocus(this); };
        }

        protected override void LoadFile(string fileName)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));

            configurator.Load(fileName);
        }

        protected override void OnClose()
        {
            Notifications.RaiseUnloaded(this, configurator);
            base.OnClose();
        }

        protected override void SaveFile(string fileName)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));

            configurator.Save(fileName);
        }
    }
}
