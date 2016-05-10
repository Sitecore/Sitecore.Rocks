// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Shell.Panes
{
    // ReSharper disable ValueAnalysis
    // ReSharper disable InconsistentNaming
    // ReSharper disable DoNotCallOverridableMethodsInConstructor
    // ReSharper disable UseObjectOrCollectionInitializer
    // ReSharper disable VirtualMemberNeverOverriden.Global
    // ReSharper disable UnusedParameter.Global
    // ReSharper disable UnusedMethodReturnValue.Global

    public abstract class SimpleEditorPane<TFactory, TUIControl> : WindowPane, IOleCommandTarget, IVsPersistDocData, IPersistFileFormat where TFactory : IVsEditorFactory where TUIControl : UserControl, new()
    {
        // Our editor will support only one file format, this is its index.

        // Character separating file format lines.

        private const char EndLineChar = (char)10;

        private const uint FileFormatIndex = 0;

        private readonly Guid commandSetGuid;

        private readonly string fileExtensionUsed;

        private string fileName;

        private bool gettingCheckoutStatus;

        private bool isDirty;

        private bool loading;

        private bool noScribbleMode;

        /* private ElementHost elementHost; */

        protected SimpleEditorPane() : base(null)
        {
            fileExtensionUsed = GetFileExtension();
            commandSetGuid = GetCommandSetGuid();

            Content = new TUIControl();
        }

        protected SimpleEditorPane([NotNull] System.IServiceProvider provider) : base(provider)
        {
            fileExtensionUsed = GetFileExtension();
            commandSetGuid = GetCommandSetGuid();

            Content = new TUIControl();
        }

        public Guid FactoryGuid
        {
            get { return typeof(TFactory).GUID; }
        }

        public string FileExtensionUsed
        {
            get { return fileExtensionUsed; }
        }

        public virtual int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            // Wrap parameters into argument type instance
            var execArgs = new ExecArgs(pguidCmdGroup, nCmdID);
            execArgs.CommandExecOpt = nCmdexecopt;
            execArgs.PvaIn = pvaIn;
            execArgs.PvaOut = pvaOut;

            if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97)
            {
                // Process standard Visual Studio Commands
                switch (nCmdID)
                {
                    case (uint)VSConstants.VSStd97CmdID.Copy:

                        // ICommonCommandSupport _UIControl.DoCopy();
                        return VSConstants.S_OK;

                    case (uint)VSConstants.VSStd97CmdID.Cut:

                        // ICommonCommandSupport _UIControl.DoCut();
                        return VSConstants.S_OK;

                    case (uint)VSConstants.VSStd97CmdID.Paste:

                        // ICommonCommandSupport _UIControl.DoPaste();
                        return VSConstants.S_OK;

                    case (uint)VSConstants.VSStd97CmdID.Redo:

                        // ICommonCommandSupport _UIControl.DoRedo();
                        return VSConstants.S_OK;

                    case (uint)VSConstants.VSStd97CmdID.Undo:

                        // ICommonCommandSupport _UIControl.DoUndo();
                        return VSConstants.S_OK;

                    case (uint)VSConstants.VSStd97CmdID.SelectAll:

                        // ICommonCommandSupport _UIControl.DoSelectAll();
                        return VSConstants.S_OK;

                    default:
                        return ExecuteVSStd97Command(execArgs) ? VSConstants.S_OK : (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;
                }
            }

            // Execute commands owned by the editor
            if (pguidCmdGroup == commandSetGuid)
            {
                return ExecuteOwnedCommand(execArgs) ? VSConstants.S_OK : (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;
            }

            // Execute any other command
            return ExecuteCommand(execArgs) ? VSConstants.S_OK : (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;
        }

        public virtual int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, [CanBeNull] OLECMD[] prgCmds, IntPtr pCmdText)
        {
            // Validate parameters
            if (prgCmds == null || cCmds != 1)
            {
                return VSConstants.E_INVALIDARG;
            }

            // Wrap parameters into argument type instance
            var statusArgs = new QueryStatusArgs(pguidCmdGroup);
            statusArgs.CommandCount = cCmds;
            statusArgs.Commands = prgCmds;
            statusArgs.PCmdText = pCmdText;

            // By default all commands are supported
            var cmdf = OLECMDF.OLECMDF_SUPPORTED;
            if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97)
            {
                // Process standard Commands
                switch (prgCmds[0].cmdID)
                {
                    case (uint)VSConstants.VSStd97CmdID.SelectAll:

                        // ICommonCommandSupport if (_UIControl.SupportsSelectAll) cmdf |= OLECMDF.OLECMDF_ENABLED;
                        break;

                    case (uint)VSConstants.VSStd97CmdID.Copy:

                        // ICommonCommandSupport if (_UIControl.SupportsCopy) cmdf |= OLECMDF.OLECMDF_ENABLED;
                        break;

                    case (uint)VSConstants.VSStd97CmdID.Cut:

                        // ICommonCommandSupport if (_UIControl.SupportsCut) cmdf |= OLECMDF.OLECMDF_ENABLED;
                        break;

                    case (uint)VSConstants.VSStd97CmdID.Paste:

                        // ICommonCommandSupport if (_UIControl.SupportsPaste) cmdf |= OLECMDF.OLECMDF_ENABLED;
                        break;

                    case (uint)VSConstants.VSStd97CmdID.Redo:

                        // ICommonCommandSupport if (_UIControl.SupportsRedo) cmdf |= OLECMDF.OLECMDF_ENABLED;
                        break;

                    case (uint)VSConstants.VSStd97CmdID.Undo:

                        // ICommonCommandSupport if (_UIControl.SupportsUndo) cmdf |= OLECMDF.OLECMDF_ENABLED;
                        break;

                    default:
                        if (!SupportsVSStd97Command(prgCmds[0].cmdID, ref cmdf))
                        {
                            return (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;
                        }

                        break;
                }

                // Pass back the commmand support flag
                prgCmds[0].cmdf = (uint)cmdf;
                return VSConstants.S_OK;
            }

            // Check for commands owned by the editor
            if (pguidCmdGroup == commandSetGuid)
            {
                return SupportsOwnedCommand(statusArgs) ? VSConstants.S_OK : (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;
            }

            // Check for any other commands
            return SupportsCommand(statusArgs) ? VSConstants.S_OK : (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;
        }

        int IPersist.GetClassID(out Guid pClassID)
        {
            pClassID = FactoryGuid;
            return VSConstants.S_OK;
        }

        int IPersistFileFormat.GetClassID(out Guid pClassID)
        {
            pClassID = FactoryGuid;
            return VSConstants.S_OK;
        }

        int IPersistFileFormat.GetCurFile([NotNull] out string ppszFilename, out uint pnFormatIndex)
        {
            return OnGetCurFile(out ppszFilename, out pnFormatIndex);
        }

        int IPersistFileFormat.GetFormatList([NotNull] out string ppszFormatList)
        {
            return OnGetFormatList(out ppszFormatList);
        }

        int IPersistFileFormat.InitNew(uint nFormatIndex)
        {
            return OnInitNew(nFormatIndex);
        }

        int IPersistFileFormat.IsDirty(out int pfIsDirty)
        {
            return OnIsDirty(out pfIsDirty);
        }

        int IPersistFileFormat.Load([CanBeNull] string pszFilename, uint grfMode, int fReadOnly)
        {
            // A valid file name is required.
            if ((pszFilename == null) && ((fileName == null) || (fileName.Length == 0)))
            {
                throw new ArgumentNullException(@"pszFilename");
            }

            loading = true;
            var hr = VSConstants.S_OK;
            try
            {
                // If the new file name is null, then this operation is a reload
                var isReload = false;
                if (pszFilename == null)
                {
                    isReload = true;
                }

                // Show the wait cursor while loading the file
                VsUIShell.SetWaitCursor();

                // Set the new file name
                if (!isReload)
                {
                    // Unsubscribe from the notification of the changes in the previous file.
                    fileName = pszFilename;
                }

                // Load the file
                LoadFile(fileName);
                isDirty = false;

                // Notify the load or reload
                NotifyDocChanged();
            }
            finally
            {
                loading = false;
            }

            return hr;
        }

        int IPersistFileFormat.Save([CanBeNull] string pszFilename, int fRemember, uint nFormatIndex)
        {
            // switch into the NoScribble mode
            noScribbleMode = true;
            try
            {
                // If file is null or same --> SAVE
                if (pszFilename == null || pszFilename == fileName)
                {
                    SaveFile(fileName);
                    isDirty = false;
                }
                else
                {
                    // If remember --> SaveAs 
                    if (fRemember != 0)
                    {
                        fileName = pszFilename;
                        SaveFile(fileName);
                        isDirty = false;
                    }
                    else
                    {
                        // Else, Save a Copy As
                        SaveFile(pszFilename);
                    }
                }
            }
            finally
            {
                // Switch into the Normal mode
                noScribbleMode = false;
            }

            return VSConstants.S_OK;
        }

        int IPersistFileFormat.SaveCompleted([NotNull] string fileName)
        {
            return OnSaveCompleted(fileName);
        }

        int IVsPersistDocData.Close()
        {
            return OnCloseEditor();
        }

        int IVsPersistDocData.GetGuidEditorType(out Guid pClassID)
        {
            pClassID = FactoryGuid;
            return VSConstants.S_OK;
        }

        int IVsPersistDocData.IsDocDataDirty(out int pfDirty)
        {
            return ((IPersistFileFormat)this).IsDirty(out pfDirty);
        }

        int IVsPersistDocData.IsDocDataReloadable(out int pfReloadable)
        {
            // Allow file to be reloaded
            pfReloadable = 1;
            return VSConstants.S_OK;
        }

        int IVsPersistDocData.LoadDocData([NotNull] string pszMkDocument)
        {
            return ((IPersistFileFormat)this).Load(pszMkDocument, 0, 0);
        }

        int IVsPersistDocData.OnRegisterDocData(uint docCookie, [NotNull] IVsHierarchy pHierNew, uint itemidNew)
        {
            return VSConstants.S_OK;
        }

        int IVsPersistDocData.ReloadDocData(uint grfFlags)
        {
            return ((IPersistFileFormat)this).Load(null, grfFlags, 0);
        }

        int IVsPersistDocData.RenameDocData(uint grfAttribs, [NotNull] IVsHierarchy pHierNew, uint itemidNew, [NotNull] string pszMkDocumentNew)
        {
            return VSConstants.S_OK;
        }

        [SuppressMessage(@"Microsoft.Globalization", @"CA1303:DoNotPassLiteralsAsLocalizedParameters", Justification = @"It's OK")]
        int IVsPersistDocData.SaveDocData(VSSAVEFLAGS dwSave, [CanBeNull] out string pbstrMkDocumentNew, out int pfSaveCanceled)
        {
            pbstrMkDocumentNew = null;
            pfSaveCanceled = 0;
            int hr;

            switch (dwSave)
            {
                case VSSAVEFLAGS.VSSAVE_Save:
                case VSSAVEFLAGS.VSSAVE_SilentSave:
                {
                    var queryEditQuerySave = (IVsQueryEditQuerySave2)GetService(typeof(SVsQueryEditQuerySave));

                    // Call QueryEditQuerySave
                    uint result;
                    hr = queryEditQuerySave.QuerySaveFile(fileName, // filename
                        0, // flags
                        null, // file attributes
                        out result); // result

                    if (ErrorHandler.Failed(hr))
                    {
                        return hr;
                    }

                    // Process according to result from QuerySave
                    switch ((tagVSQuerySaveResult)result)
                    {
                        case tagVSQuerySaveResult.QSR_NoSave_Cancel:

                            // Note that this is also case tagVSQuerySaveResult.QSR_NoSave_UserCanceled because these
                            // two tags have the same value.
                            pfSaveCanceled = ~0;
                            break;

                        case tagVSQuerySaveResult.QSR_SaveOK:

                            // Call the shell to do the save for us
                            hr = VsUIShell.SaveDocDataToFile(dwSave, this, fileName, out pbstrMkDocumentNew, out pfSaveCanceled);
                            if (ErrorHandler.Failed(hr))
                            {
                                return hr;
                            }

                            break;

                        case tagVSQuerySaveResult.QSR_ForceSaveAs:

                            // Call the shell to do the SaveAS for us
                            hr = VsUIShell.SaveDocDataToFile(VSSAVEFLAGS.VSSAVE_SaveAs, this, fileName, out pbstrMkDocumentNew, out pfSaveCanceled);
                            if (ErrorHandler.Failed(hr))
                            {
                                return hr;
                            }

                            break;

                        case tagVSQuerySaveResult.QSR_NoSave_Continue:

                            // In this case there is nothing to do.
                            break;

                        default:
                            throw new COMException();
                    }

                    break;
                }

                case VSSAVEFLAGS.VSSAVE_SaveAs:
                case VSSAVEFLAGS.VSSAVE_SaveCopyAs:
                {
                    // Make sure the file name as the right extension
                    if (string.Compare(FileExtensionUsed, Path.GetExtension(fileName), true, CultureInfo.CurrentCulture) != 0)
                    {
                        fileName += FileExtensionUsed;
                    }

                    // Call the shell to do the save for us
                    hr = VsUIShell.SaveDocDataToFile(dwSave, this, fileName, out pbstrMkDocumentNew, out pfSaveCanceled);
                    if (ErrorHandler.Failed(hr))
                    {
                        return hr;
                    }

                    break;
                }

                default:
                    throw new ArgumentException();
            }

            return VSConstants.S_OK;
        }

        int IVsPersistDocData.SetUntitledDocPath([NotNull] string pszDocDataPath)
        {
            return ((IPersistFileFormat)this).InitNew(FileFormatIndex);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (Content != null)
                    {
                        Content = null;
                    }

                    GC.SuppressFinalize(this);
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        protected virtual bool ExecuteCommand([NotNull] ExecArgs execArgs)
        {
            return false;
        }

        protected virtual bool ExecuteOwnedCommand([NotNull] ExecArgs execArgs)
        {
            return false;
        }

        protected virtual bool ExecuteVSStd97Command([CanBeNull] ExecArgs execArgs)
        {
            return false;
        }

        protected abstract Guid GetCommandSetGuid();

        protected abstract string GetFileExtension();

        protected abstract void LoadFile([NotNull] string fileName);

        protected virtual int OnCloseEditor()
        {
            return VSConstants.S_OK;
        }

        protected virtual void OnContentChanged()
        {
            // During the load operation the text of the control will change, but
            // this change must not be stored in the status of the document.
            if (!loading)
            {
                // The only interesting case is when we are changing the document
                // for the first time
                if (!isDirty)
                {
                    // Check if the QueryEditQuerySave service allow us to change the file
                    if (!CanEditFile())
                    {
                        // We can not change the file (e.g. a checkout operation failed),
                        // so undo the change and exit.
                        // ICommonCommandSupport if (_UIControl.SupportsUndo) _UIControl.DoUndo();
                        return;
                    }

                    // It is possible to change the file, so update the status.
                    isDirty = true;
                }
            }
        }

        protected virtual int OnGetCurFile([NotNull] out string ppszFilename, out uint pnFormatIndex)
        {
            // We only support 1 format so return its index
            pnFormatIndex = FileFormatIndex;
            ppszFilename = fileName;
            return VSConstants.S_OK;
        }

        protected virtual int OnGetFormatList([NotNull] out string ppszFormatList)
        {
            var formatList = string.Format(CultureInfo.CurrentCulture, @"Editor Files (*{0}){1}*{0}{1}{1}", FileExtensionUsed, EndLineChar);
            ppszFormatList = formatList;
            return VSConstants.S_OK;
        }

        protected virtual int OnInitNew(uint nFormatIndex)
        {
            if (nFormatIndex != FileFormatIndex)
            {
                throw new ArgumentException();
            }

            // Until someone change the file, we can consider it not dirty as
            // the user would be annoyed if we prompt him to save an empty file
            isDirty = false;
            return VSConstants.S_OK;
        }

        protected virtual int OnIsDirty(out int pfIsDirty)
        {
            pfIsDirty = isDirty ? 1 : 0;
            return VSConstants.S_OK;
        }

        protected virtual int OnReload()
        {
            isDirty = false;
            return VSConstants.S_OK;
        }

        protected virtual int OnSaveCompleted([NotNull] string pszFilename)
        {
            return noScribbleMode ? VSConstants.S_FALSE : VSConstants.S_OK;
        }

        protected abstract void SaveFile([NotNull] string fileName);

        protected virtual bool SupportsCommand([NotNull] QueryStatusArgs queryStatusArgs)
        {
            return false;
        }

        protected virtual bool SupportsOwnedCommand([NotNull] QueryStatusArgs queryStatusArgs)
        {
            return false;
        }

        protected virtual bool SupportsVSStd97Command(uint commandID, ref OLECMDF status)
        {
            return false;
        }

        private bool CanEditFile()
        {
            // Check the status of the recursion guard
            if (gettingCheckoutStatus)
            {
                return false;
            }

            try
            {
                // Set the recursion guard
                gettingCheckoutStatus = true;

                // Get the QueryEditQuerySave service
                var queryEditQuerySave = (IVsQueryEditQuerySave2)GetService(typeof(SVsQueryEditQuerySave));
                if (queryEditQuerySave == null)
                {
                    return false;
                }

                // Now call the QueryEdit method to find the edit status of this file
                string[] documents =
                {
                    fileName
                };
                uint result;
                uint outFlags;

                // This function can pop up a dialog to ask the user to checkout the file.
                // When this dialog is visible, it is possible to receive other request to change
                // the file and this is the reason for the recursion guard.
                var hr = queryEditQuerySave.QueryEditFiles(0, // Flags
                    1, // Number of elements in the array
                    documents, // Files to edit
                    null, // Input flags
                    null, // Input array of VSQEQS_FILE_ATTRIBUTE_DATA
                    out result, // result of the checkout
                    out outFlags); // Additional flags

                if (ErrorHandler.Succeeded(hr) && (result == (uint)tagVSQueryEditResult.QER_EditOK))
                {
                    // In this case (and only in this case) we can return true from this function.
                    return true;
                }
            }
            finally
            {
                gettingCheckoutStatus = false;
            }

            return false;
        }

        private void NotifyDocChanged()
        {
            // Make sure that we have a file name
            if (fileName.Length == 0)
            {
                return;
            }

            // Get a reference to the Running Document Table
            var runningDocTable = (IVsRunningDocumentTable)GetService(typeof(SVsRunningDocumentTable));

            // Lock the document
            uint docCookie;
            IVsHierarchy hierarchy;
            uint itemID;
            IntPtr docData;
            var hr = runningDocTable.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, fileName, out hierarchy, out itemID, out docData, out docCookie);
            ErrorHandler.ThrowOnFailure(hr);

            // Send the notification
            hr = runningDocTable.NotifyDocumentChanged(docCookie, (uint)__VSRDTATTRIB.RDTA_DocDataReloaded);

            // Unlock the document.
            // Note that we have to unlock the document even if the previous call failed.
            runningDocTable.UnlockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, docCookie);

            // Check Off the call to NotifyDocChanged failed.
            ErrorHandler.ThrowOnFailure(hr);
        }

        // ReSharper restore UnusedMethodReturnValue.Global
        // ReSharper restore UnusedParameter.Global
        // ReSharper restore VirtualMemberNeverOverriden.Global
        // ReSharper restore UseObjectOrCollectionInitializer
        // ReSharper restore DoNotCallOverridableMethodsInConstructor
        // ReSharper restore InconsistentNaming
        // ReSharper restore ValueAnalysis
    }
}
