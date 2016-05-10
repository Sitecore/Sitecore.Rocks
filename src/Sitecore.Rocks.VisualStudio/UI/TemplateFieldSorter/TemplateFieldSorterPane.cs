// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Shell.Panes;

namespace Sitecore.Rocks.UI.TemplateFieldSorter
{
    [Guid(@"e4dd5103-c96e-47bc-be9a-e3adddb75994")]
    public class TemplateFieldSorterPane : EditorPane<TemplateFieldSorterFactory, TemplateFieldSorter>, IEditorPane
    {
        private TemplateFieldSorter _templateFieldSorter;

        public void Close()
        {
        }

        public void Initialize([NotNull] ItemUri templateUri)
        {
            Assert.ArgumentNotNull(templateUri, nameof(templateUri));

            _templateFieldSorter.Initialize(templateUri);
        }

        public void SetModifiedFlag(bool flag)
        {
            if (flag)
            {
                SetModified();
            }
            else
            {
                OnReload();
            }
        }

        protected override void Initialize()
        {
            base.Initialize();

            _templateFieldSorter = (TemplateFieldSorter)Content;
            _templateFieldSorter.Pane = this;
            _templateFieldSorter.GotFocus += delegate { SitecorePackage.Instance.GotFocus(this); };
        }

        protected override void OnClose()
        {
            Notifications.RaiseUnloaded(this, _templateFieldSorter);
            base.OnClose();
        }

        protected override void SaveFile(string fileName)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));

            _templateFieldSorter.Save();
        }
    }
}
