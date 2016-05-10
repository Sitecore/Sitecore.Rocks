// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Shell.Panes;

namespace Sitecore.Rocks.UI.TemplateFieldSorter
{
    [Guid(GuidList.TemplateFieldSorterFactoryString)]
    public class TemplateFieldSorterFactory : EditorFactory<TemplateFieldSorterPane>
    {
        [CanBeNull]
        public static TemplateFieldSorterPane CreateEditor([NotNull] string documentName)
        {
            Assert.ArgumentNotNull(documentName, nameof(documentName));

            documentName = EditorDocumentName.GetDocumentName(@"TemplateSorter" + documentName);

            var windowFrame = CreateFrame(documentName, GuidList.TemplateFieldSorterFactoryString);
            if (windowFrame == null)
            {
                return null;
            }

            windowFrame.Show();

            object value;
            windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out value);

            var pane = value as TemplateFieldSorterPane;
            if (pane != null)
            {
                pane.Frame = windowFrame;
            }

            return pane;
        }
    }
}
