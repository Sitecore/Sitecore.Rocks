// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.BitmapImageExtensions;
using Sitecore.Rocks.Extensions.ContextMenuExtensions;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Media;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("image"), FieldControl("thumbnail")]
    public partial class ImageField : IReusableFieldControl, ISupportsXmlOperations
    {
        // <image mediapath="/Images/Image 1" alt="" width="" height="" hspace="" vspace="" showineditor="1" usethumbnail="1" src="/upload/1.jpg" mediaid="{78E5BAF6-3EAD-43C5-9E95-81A3186991A1}" />

        private Field sourceField;

        private string value;

        public ImageField()
        {
            InitializeComponent();

            Clear();
        }

        [NotNull]
        public string AltText { get; set; }

        [NotNull]
        public string Hspace { get; set; }

        [NotNull]
        public string ImageHeight { get; set; }

        [NotNull]
        public string ImageWidth { get; set; }

        [NotNull]
        public string MediaPath { get; set; }

        [NotNull]
        public ItemUri MediaUri { get; set; }

        [NotNull]
        public string ShowInEditor { get; set; }

        [NotNull]
        public string Src { get; set; }

        [NotNull]
        public string UseThumbnail { get; set; }

        [NotNull]
        public string Vspace { get; set; }

        protected int DisableEvents { get; set; }

        public void BrowseImage([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            var dialog = new SelectItemDialog();
            DatabaseUri databaseUri;

            if (MediaUri == ItemUri.Empty)
            {
                databaseUri = sourceField.FieldUris.First().DatabaseUri;
                dialog.Initialize(Rocks.Resources.Browse, databaseUri, "/sitecore/media library");
            }
            else
            {
                databaseUri = MediaUri.DatabaseUri;
                dialog.Initialize(Rocks.Resources.Browse, MediaUri);
            }

            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var element = response.ToXElement();
                if (element == null)
                {
                    return;
                }

                var itemHeader = ItemHeader.Parse(databaseUri, element);

                Update(itemHeader);
            };

            AppHost.Server.Items.GetItemHeader(dialog.SelectedItemUri, completed);
        }

        public Control GetFocusableControl()
        {
            return AltTextField;
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
            ErrorTextBlock.Visibility = Visibility.Collapsed;
            ErrorTextBlock.Text = string.Empty;

            Uri uri;
            if (MediaUri == ItemUri.Empty)
            {
                uri = new Uri(@"pack://application:,,,/Sitecore.Rocks;component/Resources/128x128/selection.png");
            }
            else
            {
                uri = new Uri(MediaManager.GetMediaUrl(MediaUri.Site) + MediaUri.ItemId.ToShortId() + @".ashx?bc=White&db=" + MediaUri.DatabaseName.Name + @"&h=80&la=en&thn=1");
            }

            try
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.DownloadFailed += HandleDownloadFailed;
                bitmapImage.DecodeFailed += (sender, e) => HandleDecodeFailed(sender, e, uri.ToString());

                bitmapImage.LoadIgnoreCache(uri);

                Image.Source = bitmapImage;
            }
            catch (OutOfMemoryException)
            {
                AppHost.MessageBox("Ouch, the image is too big to fit inside your computer.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                Image.Source = Icon.Empty.GetSource();
            }
        }

        public void SetField(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            this.sourceField = sourceField;
            value = sourceField.Value;
        }

        public void SetModifiedFlag()
        {
            var modified = ValueModified;

            if (modified != null)
            {
                modified();
            }
        }

        public void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, nameof(value));

            var changed = this.value != value;

            this.value = value;

            ParseValue();

            ReloadImage();
            Display();

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
            ErrorTextBlock.Visibility = Visibility.Collapsed;
            ErrorTextBlock.Text = string.Empty;
        }

        public void Update([NotNull] ItemHeader header)
        {
            Debug.ArgumentNotNull(header, nameof(header));

            var mediaPath = header.Path;
            if (mediaPath.StartsWith(@"/sitecore/media library", StringComparison.OrdinalIgnoreCase))
            {
                mediaPath = mediaPath.Mid(23);
            }

            MediaUri = header.ItemUri;
            MediaPath = mediaPath;
            Src = @"~/media" + mediaPath;

            var itemData = header as IItemData;
            AltTextField.Text = itemData.GetData(@"ex.alt.text");

            UpdateValue();
            ReloadImage();
            SetModifiedFlag();
            UpdatePath();
        }

        public void UpdateValue()
        {
            value = string.Format(@"<image mediapath=""{0}"" alt=""{1}"" width=""{2}"" height=""{3}"" hspace=""{4}"" vspace=""{5}"" showineditor=""{6}"" usethumbnail=""{7}"" src=""{8}"" mediaid=""{9}"" />", MediaPath, AltText, ImageWidth, ImageHeight, Hspace, Vspace, ShowInEditor, UseThumbnail, Src, MediaUri.ItemId);
        }

        public event ValueModifiedEventHandler ValueModified;

        private void Clear()
        {
            MediaUri = ItemUri.Empty;
            MediaPath = string.Empty;
            AltText = string.Empty;
            ImageWidth = string.Empty;
            ImageHeight = string.Empty;
            Hspace = string.Empty;
            Vspace = string.Empty;
            ShowInEditor = string.Empty;
            UseThumbnail = string.Empty;
            Src = string.Empty;
        }

        private void Commit()
        {
            AltText = AltTextField.Text;
            ImageWidth = WidthField.Text;
            ImageHeight = HeightField.Text;
            Hspace = HSpaceField.Text;
            Vspace = VSpaceField.Text;

            MediaPath = string.Empty;
            Src = string.Empty;

            UpdateValue();
            SetModifiedFlag();
        }

        private void Display()
        {
            DisableEvents++;

            AltTextField.Text = AltText;
            WidthField.Text = ImageWidth;
            HeightField.Text = ImageHeight;
            HSpaceField.Text = Hspace;
            VSpaceField.Text = Vspace;

            DisableEvents--;
        }

        Control IFieldControl.GetControl()
        {
            return this;
        }

        private void HandleDownloadFailed(object sender, [NotNull] ExceptionEventArgs e)
        {
            var uri = new Uri(@"pack://application:,,,/Sitecore.Rocks;component/Resources/128x128/delete.png");

            ErrorTextBlock.Visibility = Visibility.Visible;
            ErrorTextBlock.Text = "Failed to load image: " + e.ErrorException.Message;

            try
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.LoadIgnoreCache(uri);
                Image.Source = bitmapImage;
            }
            catch (OutOfMemoryException)
            {
                Image.Source = Icon.Empty.GetSource();
            }
        }

        private void HandleDecodeFailed([CanBeNull] object sender, [NotNull] ExceptionEventArgs e, [NotNull] string url)
        {
            var uri = new Uri(@"pack://application:,,,/Sitecore.Rocks;component/Resources/128x128/delete.png");

            ErrorTextBlock.Visibility = Visibility.Visible;
            ErrorTextBlock.Text = "The image could be not decoded. Please check security settings on this URL: " + url;

            try
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.LoadIgnoreCache(uri);
                Image.Source = bitmapImage;
            }
            catch (OutOfMemoryException)
            {
                Image.Source = Icon.Empty.GetSource();
            }
        }

        private void HandleDragOver([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Effects = DragDropEffects.None;
            e.Handled = true;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var fieldUri = sourceField.FieldUris.First();

                if ((fieldUri.Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Execute) == DataServiceFeatureCapabilities.Execute)
                {
                    e.Effects = DragDropEffects.Copy;
                }

                return;
            }

            if (e.Data.GetDataPresent(@"CF_VSSTGPROJECTITEMS") && e.Data.GetDataPresent(@"Text"))
            {
                var fileName = e.Data.GetData(@"Text") as string ?? string.Empty;

                if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
                {
                    e.Effects = DragDropEffects.Copy;
                }

                return;
            }

            if (e.Data.GetDataPresent(DragManager.DragIdentifier))
            {
                var baseItems = e.Data.GetData(DragManager.DragIdentifier) as IEnumerable<IItem>;
                if (baseItems == null)
                {
                    return;
                }

                if (baseItems.Count() != 1)
                {
                    return;
                }

                var item = baseItems.First() as ITemplatedItem;
                if (item == null)
                {
                    return;
                }

                if (IdManager.GetTemplateType(item.TemplateId) != "media")
                {
                    return;
                }

                e.Effects = DragDropEffects.Copy;
            }
        }

        private void HandleDrop([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                HandleFileDrop(e);
                return;
            }

            if (e.Data.GetDataPresent(@"CF_VSSTGPROJECTITEMS"))
            {
                HandleSolutionExplorerDrop(e);
                return;
            }

            if (e.Data.GetDataPresent(DragManager.DragIdentifier))
            {
                HandleItemDrop(e);
            }
        }

        private void HandleFieldChanged([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (DisableEvents > 0)
            {
                return;
            }

            Commit();
        }

        private void HandleFileDrop([NotNull] DragEventArgs e)
        {
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

            UploadFiles(e, droppedFilePaths[0]);
        }

        private void HandleItemDrop([NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            e.Handled = true;

            var baseItems = e.Data.GetData(DragManager.DragIdentifier) as IEnumerable<IItem>;
            if (baseItems == null)
            {
                return;
            }

            if (baseItems.Count() != 1)
            {
                return;
            }

            var item = baseItems.First() as ITemplatedItem;
            if (item == null)
            {
                return;
            }

            if (IdManager.GetTemplateType(item.TemplateId) != "media")
            {
                return;
            }

            MediaUri = item.ItemUri;

            var itemTreeViewItem = item as ItemTreeViewItem;
            if (itemTreeViewItem != null)
            {
                var mediaPath = itemTreeViewItem.Item.Path;
                if (mediaPath.StartsWith(@"/sitecore/media library", StringComparison.OrdinalIgnoreCase))
                {
                    mediaPath = mediaPath.Mid(23);
                }

                MediaPath = mediaPath;
                Src = @"~/media" + mediaPath;
            }

            var itemData = item as IItemData;
            if (itemData != null)
            {
                AltTextField.Text = itemData.GetData(@"ex.alt.text");
            }

            UpdateValue();
            ReloadImage();
            SetModifiedFlag();
            UpdatePath();
        }

        private void HandleSolutionExplorerDrop([NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            var droppedFileName = e.Data.GetData(DataFormats.Text, true) as string;
            if (droppedFileName == null)
            {
                return;
            }

            UploadFiles(e, droppedFileName);
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ContextMenu = null;

            var contentEditor = this.GetAncestorOrSelf<ContentEditor>();
            if (contentEditor == null)
            {
                return;
            }

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

        private void ParseValue()
        {
            Clear();
            PathTextBlock.Text = "[Empty]";

            XDocument doc;
            try
            {
                doc = XDocument.Parse(value);
            }
            catch
            {
                return;
            }

            var root = doc.Root;
            if (root == null)
            {
                return;
            }

            var id = root.GetAttributeValue("mediaid");
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            Guid guid;
            if (!Guid.TryParse(id, out guid))
            {
                return;
            }

            MediaUri = new ItemUri(sourceField.FieldUris.First().ItemVersionUri.DatabaseUri, new ItemId(guid));
            MediaPath = root.GetAttributeValue("mediapath");
            AltText = root.GetAttributeValue("alt");
            ImageWidth = root.GetAttributeValue("width");
            ImageHeight = root.GetAttributeValue("height");
            Hspace = root.GetAttributeValue("hspace");
            Vspace = root.GetAttributeValue("vspace");
            ShowInEditor = root.GetAttributeValue("showineditor");
            UseThumbnail = root.GetAttributeValue("usethumbnail");
            Src = root.GetAttributeValue("src");

            Display();
            UpdatePath();
        }

        private void SetWidthAndHeightFromFile([NotNull] string fileName)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));

            if (!AppHost.Files.FileExists(fileName))
            {
                return;
            }

            var src = new BitmapImage();
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                src.BeginInit();
                src.StreamSource = stream;
                src.EndInit();
            }

            ImageWidth = src.PixelWidth.ToString(CultureInfo.InvariantCulture);
            ImageHeight = src.PixelHeight.ToString(CultureInfo.InvariantCulture);

            DisableEvents++;
            try
            {
                WidthField.Text = ImageWidth;
                HeightField.Text = ImageHeight;

                if (string.IsNullOrEmpty(AltTextField.Text))
                {
                    AltText = Path.GetFileNameWithoutExtension(fileName);
                    AltTextField.Text = AltText;
                }
            }
            finally
            {
                DisableEvents--;
            }
        }

        private void ToggleDetails([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Details.Visibility = Details.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            ToggleDetailsRun.Text = Details.Visibility == Visibility.Collapsed ? "More" : "Less";
        }

        private void UpdatePath()
        {
            PathTextBlock.Text = "[Image has been updated]";

            var fieldUri = sourceField.FieldUris.FirstOrDefault();
            if (fieldUri == null)
            {
                return;
            }

            PathTextBlock.Text = "[Updating image]";

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result, true))
                {
                    PathTextBlock.Text = "[Image not found]";
                    return;
                }

                var element = response.ToXElement();
                if (element == null)
                {
                    return;
                }

                var itemHeader = ItemHeader.Parse(fieldUri.DatabaseUri, element);

                PathTextBlock.Text = itemHeader.Path;
            };

            AppHost.Server.Items.GetItemHeader(MediaUri, completed);
        }

        private void UploadFiles([NotNull] DragEventArgs e, [NotNull] string droppedFileName)
        {
            Debug.ArgumentNotNull(e, nameof(e));
            Debug.ArgumentNotNull(droppedFileName, nameof(droppedFileName));

            var site = sourceField.FieldUris.First().Site;
            if ((site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Execute) != DataServiceFeatureCapabilities.Execute)
            {
                AppHost.MessageBox(string.Format(Rocks.Resources.ImageField_HandleFileDrop_, site.DataServiceName), Rocks.Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var fieldUri = sourceField.FieldUris.First();

            GetValueCompleted<ItemHeader> uploadCompleted = delegate(ItemHeader header)
            {
                SetWidthAndHeightFromFile(droppedFileName);
                Update(header);
            };

            MediaManager.Upload(fieldUri.ItemVersionUri.DatabaseUri, @"/sitecore/media library", droppedFileName, uploadCompleted);

            e.Handled = true;
        }
    }
}
