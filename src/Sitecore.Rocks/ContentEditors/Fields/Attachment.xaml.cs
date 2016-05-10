// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.BitmapImageExtensions;
using Sitecore.Rocks.Extensions.ContextMenuExtensions;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Media;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("attachment")]
    public partial class Attachment : IReusableFieldControl
    {
        private Field sourceField;

        private string value;

        public Attachment()
        {
            InitializeComponent();
        }

        public Control GetFocusableControl()
        {
            return null;
        }

        public string GetValue()
        {
            return value;
        }

        public bool IsSupported(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            return true;
        }

        public void ReloadImage()
        {
            // This might happen if attachment is detached and the content editor closed, 
            // before the server request returns.
            if (sourceField == null)
            {
                return;
            }

            var fieldUri = sourceField.FieldUris.First();

            var path = MediaManager.GetMediaUrl(fieldUri.Site) + fieldUri.ItemId.ToShortId() + @".ashx?bc=White&db=" + fieldUri.DatabaseName.Name + @"&h=80&la=en&thn=1";

            // var webClient = new WebClient();
            // webClient.Headers[FormsAuthentication.FormsCookieName] = "";
            // var data = webClient.DownloadString(path);
            // ((HardRockWebService)fieldUri.Site.DataService).DataService;
            var bitmapImage = new BitmapImage();
            bitmapImage.LoadIgnoreCache(new Uri(path));

            Image.Source = bitmapImage;
        }

        public void SetField(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            this.sourceField = sourceField;
            value = sourceField.Value;
        }

        public void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, nameof(value));

            var changed = this.value != value;

            this.value = value;

            ReloadImage();

            if (!changed)
            {
                return;
            }

            var modified = ValueModified;
            if (modified != null)
            {
                modified();
            }
        }

        public void UnsetField()
        {
            sourceField = null;
        }

        public event ValueModifiedEventHandler ValueModified;

        Control IFieldControl.GetControl()
        {
            return this;
        }

        private void HandleDragOver([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Effects = DragDropEffects.None;

            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            var fieldUri = sourceField.FieldUris.First();

            if ((fieldUri.Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Execute) == DataServiceFeatureCapabilities.Execute)
            {
                return;
            }

            e.Effects = DragDropEffects.Copy;
        }

        private void HandleDrop([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var droppedFilePaths = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            if (droppedFilePaths == null)
            {
                return;
            }

            if (droppedFilePaths.Length != 1)
            {
                AppHost.MessageBox(Rocks.Resources.Attachment_HandleDrop_You_can_only_drop_a_single_image_here_, Rocks.Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var fieldUri = sourceField.FieldUris.First();

            GetValueCompleted<bool> uploadCompleted = delegate { SetValue(value); };

            MediaManager.Attach(fieldUri.ItemVersionUri.ItemUri, @"/upload/Images/Uploaded", droppedFilePaths[0], uploadCompleted);

            e.Handled = true;
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var contentEditor = this.GetAncestorOrSelf<ContentEditor>();
            if (contentEditor == null)
            {
                return;
            }

            ContextMenu = null;

            var fld = sourceField;
            if (fld == null)
            {
                e.Handled = true;
                return;
            }

            var context = new ContentEditorFieldContext(contentEditor, fld, this);

            var commands = CommandManager.GetCommands(context).ToList();
            if (!commands.Any())
            {
                e.Handled = true;
                return;
            }

            var contextMenu = new ContextMenu();

            contextMenu.Build(commands, context);

            ContextMenu = contextMenu;
        }
    }
}
