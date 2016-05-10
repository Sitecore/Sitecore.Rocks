// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Shell.Panes;

namespace Sitecore.Rocks.ContentEditors
{
    [Guid(GuidList.ContentEditorFactoryString)]
    public class ContentEditorFactory : EditorFactory<ContentEditorPane>
    {
        [CanBeNull]
        public static ContentEditorPane CreateEditor([NotNull] string documentName, bool newTab)
        {
            Assert.ArgumentNotNull(documentName, nameof(documentName));

            var reuseWindow = AppHost.Settings.Options.ReuseWindow && !newTab;

            var doc = documentName;
            if (reuseWindow)
            {
                doc = @"ReusableItemEditor";
            }

            var windowFrame = CreateFrame(doc, GuidList.ContentEditorFactoryString);
            if (windowFrame == null)
            {
                return null;
            }

            windowFrame.Show();

            object value;
            windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out value);

            var pane = value as ContentEditorPane;
            if (pane != null)
            {
                pane.Frame = windowFrame;
            }

            return pane;
        }
    }
}
