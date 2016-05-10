// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Shell.Panes
{
    public static class VsUIShell
    {
        [NotNull]
        private static IVsUIShell UIShell
        {
            get { return (IVsUIShell)Package.GetGlobalService(typeof(Microsoft.VisualStudio.Shell.Interop.SVsUIShell)); }
        }

        public static int SaveDocDataToFile(VSSAVEFLAGS grfSave, [CanBeNull] object pPersistFile, [CanBeNull] string pszUntitledPath, [CanBeNull] out string pbstrDocumentNew, out int pfCanceled)
        {
            return UIShell.SaveDocDataToFile(grfSave, pPersistFile, pszUntitledPath, out pbstrDocumentNew, out pfCanceled);
        }

        public static void SetWaitCursor()
        {
            UIShell.SetWaitCursor();
        }
    }
}
