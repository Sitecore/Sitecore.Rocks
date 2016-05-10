// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Shell.Panes;

namespace Sitecore.Rocks.UI.Packages.PackageBuilders
{
    [Guid(GuidList.PackageBuilderFactoryString), UsedImplicitly]
    public class PackageBuilderFactory : EditorFactory<PackageBuilderPane>
    {
        [CanBeNull, UsedImplicitly]
        public static PackageBuilderPane CreateEditor([NotNull] string documentName)
        {
            Assert.ArgumentNotNull(documentName, nameof(documentName));

            documentName = EditorDocumentName.GetDocumentName(documentName);

            var windowFrame = CreateFrame(documentName, GuidList.PackageBuilderFactoryString);
            if (windowFrame == null)
            {
                return null;
            }

            windowFrame.Show();

            object value;
            windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out value);

            var pane = value as PackageBuilderPane;
            if (pane != null)
            {
                pane.Frame = windowFrame;
            }

            return pane;
        }
    }
}
