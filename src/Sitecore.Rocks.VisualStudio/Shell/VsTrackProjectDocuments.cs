// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Shell
{
    public class VsTrackProjectDocuments : IVsTrackProjectDocumentsEvents2
    {
        public VsTrackProjectDocuments()
        {
            uint cookie;

            GetTrackProjectDocuments().AdviseTrackProjectDocumentsEvents(this, out cookie);
        }

        public int OnAfterAddDirectoriesEx(int projectCount, int directories, [CanBeNull] IVsProject[] projects, [CanBeNull] int[] firstIndices, [CanBeNull] string[] documents, [CanBeNull] VSADDDIRECTORYFLAGS[] flags)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterAddFilesEx(int projectCount, int fileCount, [CanBeNull] IVsProject[] projects, [CanBeNull] int[] firstIndices, [CanBeNull] string[] documents, [CanBeNull] VSADDFILEFLAGS[] flags)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterRemoveDirectories(int projectCount, int directories, [CanBeNull] IVsProject[] projects, [CanBeNull] int[] firstIndices, [CanBeNull] string[] documents, [CanBeNull] VSREMOVEDIRECTORYFLAGS[] flags)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterRemoveFiles(int projectCount, int fileCount, [CanBeNull] IVsProject[] projects, [CanBeNull] int[] firstIndices, [CanBeNull] string[] documents, [CanBeNull] VSREMOVEFILEFLAGS[] flags)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterRenameDirectories(int projectCount, int directories, [CanBeNull] IVsProject[] projects, [CanBeNull] int[] firstIndices, [CanBeNull] string[] rgszMkOldNames, [CanBeNull] string[] rgszMkNewNames, [CanBeNull] VSRENAMEDIRECTORYFLAGS[] flags)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterRenameFiles(int projectCount, int fileCount, [CanBeNull] IVsProject[] projects, [CanBeNull] int[] firstIndices, [CanBeNull] string[] oldNames, [CanBeNull] string[] newNames, [CanBeNull] VSRENAMEFILEFLAGS[] flags)
        {
            if (newNames == null)
            {
                return VSConstants.S_OK;
            }

            if (oldNames == null)
            {
                return VSConstants.S_OK;
            }

            for (var n = 0; n < fileCount; n++)
            {
                var newName = newNames[n];
                var oldName = oldNames[n];

                ShellNotifications.RaiseProjectItemMoved(this, newName, oldName);
            }

            return VSConstants.S_OK;
        }

        public int OnAfterSccStatusChanged(int projectCount, int fileCount, [CanBeNull] IVsProject[] projects, [CanBeNull] int[] firstIndices, [CanBeNull] string[] documents, [CanBeNull] uint[] rgdwSccStatus)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryAddDirectories([CanBeNull] IVsProject project, int directories, [CanBeNull] string[] documents, [CanBeNull] VSQUERYADDDIRECTORYFLAGS[] flags, [CanBeNull] VSQUERYADDDIRECTORYRESULTS[] summaryResult, [CanBeNull] VSQUERYADDDIRECTORYRESULTS[] results)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryAddFiles([CanBeNull] IVsProject project, int fileCount, [CanBeNull] string[] documents, [CanBeNull] VSQUERYADDFILEFLAGS[] flags, [CanBeNull] VSQUERYADDFILERESULTS[] summaryResult, [CanBeNull] VSQUERYADDFILERESULTS[] results)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryRemoveDirectories([CanBeNull] IVsProject project, int directories, [CanBeNull] string[] documents, [CanBeNull] VSQUERYREMOVEDIRECTORYFLAGS[] flags, [CanBeNull] VSQUERYREMOVEDIRECTORYRESULTS[] summaryResult, [CanBeNull] VSQUERYREMOVEDIRECTORYRESULTS[] results)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryRemoveFiles([CanBeNull] IVsProject project, int fileCount, [CanBeNull] string[] documents, [CanBeNull] VSQUERYREMOVEFILEFLAGS[] flags, [CanBeNull] VSQUERYREMOVEFILERESULTS[] summaryResult, [CanBeNull] VSQUERYREMOVEFILERESULTS[] results)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryRenameDirectories([CanBeNull] IVsProject project, int directories, [CanBeNull] string[] oldNames, [CanBeNull] string[] newNames, [CanBeNull] VSQUERYRENAMEDIRECTORYFLAGS[] flags, [CanBeNull] VSQUERYRENAMEDIRECTORYRESULTS[] summaryResult, [CanBeNull] VSQUERYRENAMEDIRECTORYRESULTS[] results)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryRenameFiles([CanBeNull] IVsProject project, int fileCount, [CanBeNull] string[] oldNames, [CanBeNull] string[] newNames, [CanBeNull] VSQUERYRENAMEFILEFLAGS[] flags, [CanBeNull] VSQUERYRENAMEFILERESULTS[] summaryResult, [CanBeNull] VSQUERYRENAMEFILERESULTS[] results)
        {
            return VSConstants.S_OK;
        }

        [NotNull]
        private IVsTrackProjectDocuments2 GetTrackProjectDocuments()
        {
            var sp = (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)SitecorePackage.Instance.Dte;

            var guid = typeof(SVsTrackProjectDocuments).GUID;
            var iid = typeof(IVsTrackProjectDocuments2).GUID;
            IntPtr ptrUnknown;

            sp.QueryService(ref guid, ref iid, out ptrUnknown);

            var result = (IVsTrackProjectDocuments2)Marshal.GetObjectForIUnknown(ptrUnknown);

            Marshal.Release(ptrUnknown);

            return result;
        }
    }
}
