// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Shell.Panes
{
    public class EditorFactory<TEditorPane> : SimpleEditorFactory<TEditorPane> where TEditorPane : WindowPane, IOleCommandTarget, IVsPersistDocData, IPersistFileFormat, new()
    {
        [CanBeNull]
        protected static IVsWindowFrame CreateFrame([NotNull] string documentName, [NotNull] string guid)
        {
            Debug.ArgumentNotNull(documentName, nameof(documentName));
            Debug.ArgumentNotNull(guid, nameof(guid));

			IVsUIShellOpenDocument shellOpenDocument = null;
			ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				shellOpenDocument = SitecorePackage.Instance.GetService<IVsUIShellOpenDocument>();
			});
            if (shellOpenDocument == null)
            {
                AppHost.Output.Log("Failed to get IVsUIShellOpenDocument");
                return null;
            }

            documentName = EditorDocumentName.GetDocumentName(documentName);

            var editorGuid = Guid.Parse(guid);
            var logicalViewGuid = VSConstants.LOGVIEWID_Primary;
            Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider;
            IVsUIHierarchy hierarchy;
            uint pitemid;
            IVsWindowFrame windowFrame;

            var result = shellOpenDocument.OpenDocumentViaProjectWithSpecific(documentName, (uint)__VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_DoOpen, ref editorGuid, null, ref logicalViewGuid, out serviceProvider, out hierarchy, out pitemid, out windowFrame);

            if (windowFrame == null)
            {
                AppHost.Output.Log("Failed to create VS Frame: " + result);
            }

            return windowFrame;
        }
    }
}
