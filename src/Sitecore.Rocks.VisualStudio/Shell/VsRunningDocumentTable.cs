// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Shell
{
    public class VsRunningDocumentTable : IVsRunningDocTableEvents
    {
        public VsRunningDocumentTable()
        {
            uint cookie;

            var rdt = Package.GetGlobalService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
            if (rdt == null)
            {
                AppHost.Output.Log("Could not get IVsRunningDocumentTable");
                return;
            }

            rdt.AdviseRunningDocTableEvents(this, out cookie);
        }

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return 0;
        }

        public int OnAfterDocumentWindowHide(uint docCookie, [CanBeNull] IVsWindowFrame pFrame)
        {
            return 0;
        }

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return 0;
        }

        public int OnAfterSave(uint docCookie)
        {
            var rdt = Package.GetGlobalService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
            if (rdt == null)
            {
                AppHost.Output.Log("Could not get IVsRunningDocumentTable");
                return 0;
            }

            uint flags;
            uint readLocks;
            uint editLocks;
            string fileName;
            IVsHierarchy hierarchyItem;
            uint itemId;
            IntPtr docData;
            if (rdt.GetDocumentInfo(docCookie, out flags, out readLocks, out editLocks, out fileName, out hierarchyItem, out itemId, out docData) != VSConstants.S_OK)
            {
                return 0;
            }

            Notifications.RaiseFileSaved(fileName);
            return 0;
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, [CanBeNull] IVsWindowFrame pFrame)
        {
            return 0;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return 0;
        }
    }
}
