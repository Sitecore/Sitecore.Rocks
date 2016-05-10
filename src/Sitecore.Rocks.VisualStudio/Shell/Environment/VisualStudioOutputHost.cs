// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Shell.Environment
{
    public class VisualStudioOutputHost : OutputHost
    {
        private static Guid outputWindowGuid = new Guid("12062116-C721-408F-95A5-9D215F78206E");

        public override void Clear()
        {
            var outputPane = GetOutputPane();
            if (outputPane != null)
            {
                outputPane.Clear();
            }
        }

        public override void Show()
        {
            var outputPane = GetOutputPane();
            if (outputPane == null)
            {
                return;
            }

            var activeWindow = SitecorePackage.Instance.Dte.ActiveWindow;

            var outputWindow = SitecorePackage.Instance.Dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
            if (outputWindow == null)
            {
                return;
            }

            try
            {
                outputWindow.Visible = true;
            }
            catch (Exception ex)
            {
                AppHost.Output.LogException(ex);
                return;
            }

            if (activeWindow == null)
            {
                return;
            }

            try
            {
                activeWindow.Activate();
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }

        protected override void Write(string pane, string text)
        {
            Assert.ArgumentNotNull(text, nameof(text));

            var outputPane = GetOutputPane(pane);
            if (outputPane != null)
            {
                outputPane.OutputStringThreadSafe(text + "\r\n");
            }

            // File.AppendAllText("e:\\log.txt", text + "\r\n");
        }

        [CanBeNull]
        private IVsOutputWindowPane GetOutputPane()
        {
            return GetOutputPane(@"Sitecore Rocks");
        }

        [CanBeNull]
        private IVsOutputWindowPane GetOutputPane([NotNull] string name)
        {
            Debug.ArgumentNotNull(name, nameof(name));

            var window = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            if (window == null)
            {
                return null;
            }

            IVsOutputWindowPane outputPane;
            window.GetPane(ref outputWindowGuid, out outputPane);
            if (outputPane != null)
            {
                return outputPane;
            }

            window.CreatePane(ref outputWindowGuid, name, 1, 1);
            window.GetPane(ref outputWindowGuid, out outputPane);

            return outputPane;
        }
    }
}
