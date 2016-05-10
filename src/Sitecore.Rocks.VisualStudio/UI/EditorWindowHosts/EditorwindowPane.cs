// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Shell.Panes;

namespace Sitecore.Rocks.UI.EditorWindowHosts
{
    [Guid(@"1ddc1352-4686-4758-b61d-eeb3c98b831f")]
    public class EditorWindowPane : EditorPane<EditorWindowFactory, UserControl>, IEditorPane
    {
        public void Close()
        {
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

            Content = EditorWindowFactory.UserControl;

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

        protected override void SaveFile(string fileName)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));

            var content = Content as ISavable;
            if (content != null)
            {
                content.Save();
            }
        }
    }
}
