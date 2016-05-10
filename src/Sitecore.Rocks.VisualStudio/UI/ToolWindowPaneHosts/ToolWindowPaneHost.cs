// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.UI.ToolWindowPaneHosts
{
    [Guid("9fe3cfb7-dccc-405b-81e1-3c685b3b2308")]
    public class ToolWindowPaneHost : ToolWindowPane, IPane
    {
        public ToolWindowPaneHost() : base(null)
        {
            BitmapResourceID = 302;
            BitmapIndex = 1;
        }

        [NotNull]
        public string DocumentName { get; set; }

        [CanBeNull]
        public WindowsFormsHost Host
        {
            get { return Content as WindowsFormsHost; }
        }

        [NotNull]
        public override IWin32Window Window
        {
            get
            {
                var host = Host;
                if (host != null)
                {
                    return host.Child;
                }

                return (IWin32Window)Content;
            }
        }

        [NotNull]
        private static object Control { get; set; }

        [NotNull]
        public static ToolWindowPaneHost Show([NotNull] FrameworkElement frameworkElement, [NotNull] string documentName)
        {
            Assert.ArgumentNotNull(frameworkElement, nameof(frameworkElement));
            Assert.ArgumentNotNull(documentName, nameof(documentName));

            Control = frameworkElement;

            return Show(documentName);
        }

        [NotNull]
        public static ToolWindowPaneHost Show([NotNull] Control control, [NotNull] string documentName)
        {
            Assert.ArgumentNotNull(control, nameof(control));
            Assert.ArgumentNotNull(documentName, nameof(documentName));

            var host = new WindowsFormsHost
            {
                Child = control
            };

            Control = host;

            return Show(documentName);
        }

        protected override void Initialize()
        {
            base.Initialize();

            Content = Control;

            var element = Content as UIElement;
            if (element != null)
            {
                element.GotFocus += delegate { SitecorePackage.Instance.GotFocus(this); };
            }
        }

        protected override void OnClose()
        {
            Notifications.RaiseUnloaded(this, Content);
            base.OnClose();
        }

        [NotNull]
        private static ToolWindowPaneHost Show([NotNull] string documentName)
        {
            Debug.ArgumentNotNull(documentName, nameof(documentName));

            ToolWindowPaneHost window;

            for (var n = 0;; n++)
            {
                var w = SitecorePackage.Instance.FindToolWindow(typeof(ToolWindowPaneHost), n, false);
                if (w != null)
                {
                    var pane = w as ToolWindowPaneHost;
                    if (pane == null)
                    {
                        continue;
                    }

                    if (pane.DocumentName == documentName)
                    {
                        window = pane;
                        break;
                    }
                }

                if (w == null)
                {
                    window = (ToolWindowPaneHost)SitecorePackage.Instance.FindToolWindow(typeof(ToolWindowPaneHost), n, true);

                    window.DocumentName = documentName;
                    window.Caption = documentName;
                    break;
                }
            }

            if (window == null || window.Frame == null)
            {
                throw new NotSupportedException(Resources.ToolWindowPaneHost_Show_Can_not_create_tool_window_);
            }

            var windowFrame = (IVsWindowFrame)window.Frame;

            ErrorHandler.ThrowOnFailure(windowFrame.Show());

            return window;
        }
    }
}
