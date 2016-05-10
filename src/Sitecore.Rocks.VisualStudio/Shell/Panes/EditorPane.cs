// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Shell.Panes
{
    public abstract class EditorPane<TFactory, TControl> : SimpleEditorPane<TFactory, TControl>, IServicePane where TFactory : IVsEditorFactory where TControl : UserControl, new()
    {
        private int bitmapIndex;

        private int bitmapResourceID;

        private string caption;

        private IVsWindowFrame frame;

        private Package package;

        private CommandID toolBarCommandID;

        private IDropTarget toolBarDropTarget;

        private VSTWT_LOCATION toolBarLocation;

        private Guid toolClsid;

        protected EditorPane()
        {
            toolClsid = Guid.Empty;
            bitmapIndex = -1;
            bitmapResourceID = -1;
            toolBarLocation = VSTWT_LOCATION.VSTWT_TOP;
        }

        protected EditorPane([NotNull] System.IServiceProvider provider) : base(provider)
        {
            toolClsid = Guid.Empty;
            bitmapIndex = -1;
            bitmapResourceID = -1;
            toolBarLocation = VSTWT_LOCATION.VSTWT_TOP;
        }

        // Properties

        public int BitmapIndex
        {
            get { return bitmapIndex; }

            set
            {
                bitmapIndex = value;
                if ((frame == null) || (bitmapIndex == -1))
                {
                    return;
                }

                try
                {
                    frame.SetProperty(-5007, bitmapIndex);
                }
                catch (COMException exception)
                {
                    AppHost.Output.LogException(exception);
                }
            }
        }

        public int BitmapResourceID
        {
            get { return bitmapResourceID; }

            set
            {
                bitmapResourceID = value;
                if ((frame == null) || (bitmapResourceID == -1))
                {
                    return;
                }

                try
                {
                    frame.SetProperty(-5006, bitmapResourceID);
                }
                catch (COMException exception)
                {
                    AppHost.Output.LogException(exception);
                }
            }
        }

        [CanBeNull]
        public string Caption
        {
            get { return caption ?? string.Empty; }

            set
            {
                caption = value;

                if (frame == null || caption == null)
                {
                    return;
                }

                try
                {
                    frame.SetProperty((int)__VSFPROPID.VSFPROPID_OwnerCaption | (int)__VSFPROPID.VSFPROPID_EditorCaption, caption);
                    /*
          object obj;
          this.frame.GetProperty((int)__VSFPROPID.VSFPROPID_pszMkDocument, out obj);

          var documentName = obj as string;
          var newName = documentName;
          if (newName != null)
          {
            var n = newName.IndexOf(@"\", StringComparison.Ordinal);
            if (n >= 0)
            {
              newName = newName.Left(n);
            }

            newName += @"\" + this.Caption;

            // int result = this.frame.SetProperty((int)__VSFPROPID.VSFPROPID_pszMkDocument, newName);

            var runningDocTable = (IVsRunningDocumentTable)this.GetService(typeof(SVsRunningDocumentTable));
            uint docCookie;
            IVsHierarchy hierarchy;
            uint itemID;
            IntPtr docData;
            var hr = runningDocTable.FindAndLockDocument(
              (uint)_VSRDTFLAGS.RDT_ReadLock,
              documentName,
              out hierarchy,
              out itemID,
              out docData,
              out docCookie);

            var result = ((IVsPersistDocData)this).RenameDocData((int)__VSRDTATTRIB.RDTA_MkDocument, hierarchy, itemID, newName);

            runningDocTable.UnlockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, docCookie);
            // this.SetFileName(documentName);
            this.frame.GetProperty((int)__VSFPROPID.VSFPROPID_pszMkDocument, out obj);
          }
          */
                }
                catch (COMException exception)
                {
                    AppHost.Output.LogException(exception);
                }
            }
        }

        [CanBeNull]
        public object Frame
        {
            get { return frame; }

            set
            {
                frame = (IVsWindowFrame)value;
                OnToolWindowCreated();
            }
        }

        [CanBeNull]
        public object Package
        {
            get { return package; }

            set
            {
                if ((frame != null) || (package != null))
                {
                    throw new NotSupportedException(@"PackageOnlySetByCreator");
                }

                package = (Package)value;
            }
        }

        [CanBeNull]
        public CommandID ToolBar
        {
            get { return toolBarCommandID; }

            set
            {
                if (frame != null)
                {
                    throw new Exception(@"TooLateToAddToolbar");
                }

                toolBarCommandID = value;
            }
        }

        [CanBeNull, CLSCompliant(false)]
        public IDropTarget ToolBarDropTarget
        {
            get { return toolBarDropTarget; }

            set
            {
                if (frame != null)
                {
                    throw new Exception(@"TooLateToAddToolbar");
                }

                toolBarDropTarget = value;
            }
        }

        public int ToolBarLocation
        {
            get { return (int)toolBarLocation; }

            set
            {
                if (frame != null)
                {
                    throw new Exception(@"TooLateToAddToolbar");
                }

                toolBarLocation = (VSTWT_LOCATION)value;
            }
        }

        public Guid ToolClsid
        {
            get { return toolClsid; }

            set
            {
                if (frame != null)
                {
                    throw new Exception(@"TooLateToAddTool");
                }

                toolClsid = value;
            }
        }

        [NotNull]
        public virtual object GetIVsWindowPane()
        {
            return this;
        }

        public object GetVsService(Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            return GetService(type);
        }

        public virtual void OnToolBarAdded()
        {
        }

        public virtual void OnToolWindowCreated()
        {
            Caption = caption;
            BitmapResourceID = bitmapResourceID;
            BitmapIndex = bitmapIndex;
        }

        public void SetModified()
        {
            OnContentChanged();
        }

        protected override Guid GetCommandSetGuid()
        {
            return GuidList.CommandSet;
        }

        [NotNull]
        protected override string GetFileExtension()
        {
            return SitecorePackage.ContentEditorFileExtension;
        }

        protected override void LoadFile(string fileName)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));
        }
    }
}
