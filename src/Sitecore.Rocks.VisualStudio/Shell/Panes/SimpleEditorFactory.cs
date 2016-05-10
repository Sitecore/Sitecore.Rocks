// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Shell.Panes
{
    public class SimpleEditorFactory<TEditorPane> : IVsEditorFactory, IDisposable where TEditorPane : WindowPane, IOleCommandTarget, IVsPersistDocData, IPersistFileFormat, new()
    {
        private ServiceProvider _ServiceProvider;

        public virtual int Close()
        {
            return VSConstants.S_OK;
        }

        [EnvironmentPermission(SecurityAction.Demand, Unrestricted = true)]
        public virtual int CreateEditorInstance(uint grfCreateDoc, [NotNull] string pszMkDocument, [NotNull] string pszPhysicalView, [NotNull] IVsHierarchy pvHier, uint itemid, IntPtr punkDocDataExisting, out IntPtr ppunkDocView, out IntPtr ppunkDocData, [NotNull] out string pbstrEditorCaption, out Guid pguidCmdUI, out int pgrfCDW)
        {
            // --- Initialize to null
            ppunkDocView = IntPtr.Zero;
            ppunkDocData = IntPtr.Zero;
            pguidCmdUI = GetType().GUID;
            pgrfCDW = 0;
            pbstrEditorCaption = null;

            // --- Validate inputs
            if ((grfCreateDoc & (VSConstants.CEF_OPENFILE | VSConstants.CEF_SILENT)) == 0)
            {
                return VSConstants.E_INVALIDARG;
            }

            if (punkDocDataExisting != IntPtr.Zero)
            {
                return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
            }

            // --- Create the Document (editor)
            var newEditor = new TEditorPane();
            ppunkDocView = Marshal.GetIUnknownForObject(newEditor);
            ppunkDocData = Marshal.GetIUnknownForObject(newEditor);
            pbstrEditorCaption = string.Empty;

            return VSConstants.S_OK;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public object GetService([NotNull] Type serviceType)
        {
            return _ServiceProvider.GetService(serviceType);
        }

        public virtual int MapLogicalView(ref Guid logicalView, [NotNull] out string physicalView)
        {
            physicalView = null; // --- Initialize out parameter

            // --- We support only a single physical view
            if (VSConstants.LOGVIEWID_Primary == logicalView)
            {
                // --- Primary view uses NULL as physicalView
                return VSConstants.S_OK;
            }
            else
            {
                // --- You must return E_NOTIMPL for any unrecognized logicalView values
                return VSConstants.E_NOTIMPL;
            }
        }

        public virtual int SetSite([NotNull] Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider)
        {
            _ServiceProvider = new ServiceProvider(serviceProvider);
            return VSConstants.S_OK;
        }

        private void Dispose(bool disposing)
        {
            // --- If disposing equals true, dispose all managed and unmanaged resources
            if (disposing)
            {
                // --- Since we create a ServiceProvider which implements IDisposable we
                // --- also need to implement IDisposable to make sure that the ServiceProvider's
                // --- Dispose method gets called.
                if (_ServiceProvider != null)
                {
                    _ServiceProvider.Dispose();
                    _ServiceProvider = null;
                }
            }
        }
    }
}
