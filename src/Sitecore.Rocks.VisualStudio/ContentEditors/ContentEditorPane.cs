// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Runtime.InteropServices;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Shell.Panes;

namespace Sitecore.Rocks.ContentEditors
{
    [Guid(@"c918de4a-fb6c-4be2-8c84-c67278acbbf8")]
    public class ContentEditorPane : EditorPane<ContentEditorFactory, ContentEditor>, IEditorPane
    {
        private ContentEditor contentEditor;

        public void Close()
        {
            OnCloseEditor();
        }

        public void LoadItems([NotNull] List<ItemVersionUri> items, [NotNull] LoadItemsOptions options)
        {
            Assert.ArgumentNotNull(items, nameof(items));
            Assert.ArgumentNotNull(options, nameof(options));

            contentEditor.LoadItems(items, options);
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

            contentEditor = (ContentEditor)Content;
            contentEditor.Pane = this;

            contentEditor.GotFocus += delegate { SitecorePackage.Instance.GotFocus(this); };
        }

        protected override void OnClose()
        {
            contentEditor.ClearFieldControls(contentEditor.ContentModel);
            Notifications.RaiseUnloaded(this, contentEditor);
            base.OnClose();
        }

        protected override void SaveFile(string fileName)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));

            contentEditor.Save();
        }
    }
}
