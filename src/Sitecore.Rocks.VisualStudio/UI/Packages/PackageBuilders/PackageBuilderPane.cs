// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Projects;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Shell.Panes;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Packages.PackageBuilders
{
    [Guid(@"3871d645-1c79-4e9a-8cc2-93ee66b0d1cf")]
    public class PackageBuilderPane : EditorPane<PackageBuilderFactory, PackageBuilder>, IEditorPane
    {
        private PackageBuilder packageBuilder;

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

            packageBuilder = (PackageBuilder)Content;
            packageBuilder.Pane = this;
            packageBuilder.GotFocus += delegate { SitecorePackage.Instance.GotFocus(this); };
        }

        protected override void LoadFile(string fileName)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));

            Site site = null;

            var project = ProjectManager.FindProjectFromProjectItemFileName(fileName);
            if (project != null)
            {
                site = project.Site;
            }

            if (PackageBuilder.Load(fileName, packageBuilder, site))
            {
                return;
            }

            packageBuilder.Disable();
        }

        protected override void OnClose()
        {
            Notifications.RaiseUnloaded(this, packageBuilder);
            base.OnClose();
        }

        protected override void SaveFile(string fileName)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));

            packageBuilder.FileName = fileName;

            packageBuilder.Save();
        }
    }
}
