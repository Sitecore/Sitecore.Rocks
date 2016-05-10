// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Shell.Panes;

namespace Sitecore.Rocks.CodeGeneration
{
    [Guid(GuidList.CodeGenerationFactoryString), UsedImplicitly]
    public class CodeGenerationFactory : EditorFactory<CodeGenerationPane>
    {
        [CanBeNull, UsedImplicitly]
        public static CodeGenerationPane CreateEditor([NotNull] string documentName)
        {
            Assert.ArgumentNotNull(documentName, nameof(documentName));

            var windowFrame = CreateFrame(documentName, GuidList.CodeGenerationFactoryString);
            if (windowFrame == null)
            {
                return null;
            }

            windowFrame.Show();

            object value;
            windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out value);

            var pane = value as CodeGenerationPane;
            if (pane != null)
            {
                pane.Frame = windowFrame;
            }

            return pane;
        }
    }
}
