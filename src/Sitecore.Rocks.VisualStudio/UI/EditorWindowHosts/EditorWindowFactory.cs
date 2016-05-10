// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Shell.Panes;

namespace Sitecore.Rocks.UI.EditorWindowHosts
{
    [Guid(GuidList.EditorWindowFactoryString)]
    public class EditorWindowFactory : EditorFactory<EditorWindowPane>
    {
        [CanBeNull]
        internal static object UserControl { get; set; }

        [CanBeNull]
        public static EditorWindowPane CreateEditor([NotNull] FrameworkElement userControl, [NotNull] string document)
        {
            Assert.ArgumentNotNull(userControl, nameof(userControl));
            Assert.ArgumentNotNull(document, nameof(document));

            UserControl = userControl;

            var windowFrame = CreateFrame(document, GuidList.EditorWindowFactoryString);
            if (windowFrame == null)
            {
                return null;
            }

            windowFrame.Show();

            object value;
            windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out value);

            var pane = value as EditorWindowPane;
            if (pane != null)
            {
                pane.Frame = windowFrame;
                pane.Content = userControl;
            }

            var p = userControl as IHasEditorPane;
            if (p != null)
            {
                p.Pane = pane;
            }

            UserControl = null;

            return pane;
        }
    }
}
