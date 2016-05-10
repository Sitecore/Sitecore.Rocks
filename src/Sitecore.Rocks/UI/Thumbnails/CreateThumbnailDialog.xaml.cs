// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Drawing;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.BitmapImageExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;

namespace Sitecore.Rocks.UI.Thumbnails
{
    public partial class CreateThumbnailDialog
    {
        public CreateThumbnailDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            Loaded += ControlLoaded;
        }

        [NotNull]
        public DatabaseUri DatabaseUri { get; private set; }

        [CanBeNull]
        public string FileName { get; set; }

        [NotNull]
        public string ItemName { get; private set; }

        public int X { get; set; }

        public int Y { get; set; }

        protected bool IsMouseDown { get; set; }

        protected System.Windows.Point Offset { get; set; }

        protected System.Windows.Point Position { get; set; }

        [NotNull]
        protected BitmapImage SourceImage { get; set; }

        [NotNull]
        protected Bitmap Thumbnail { get; set; }

        public void Initialize([NotNull] DatabaseUri databaseUri, [NotNull] string itemName)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(itemName, nameof(itemName));

            DatabaseUri = databaseUri;
            ItemName = itemName;

            Clear();

            EnableButtons();
        }

        protected void Clear()
        {
            Preview.Source = null;
            Preview128x128.Source = null;
            Preview48x48.Source = null;
            Preview32x32.Source = null;
            Preview24x24.Source = null;
            Preview16x16.Source = null;

            X = 0;
            Y = 0;

            RubberBand.Margin = new Thickness(X, Y, 0, 0);
            RubberBand.Visibility = Visibility.Collapsed;
        }

        private void Browse([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var dialog = new SelectItemDialog();

            dialog.Initialize(Rocks.Resources.Browse, new ItemUri(DatabaseUri, new ItemId(DatabaseTreeViewItem.RootItemGuid)));

            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            dialog.GetSelectedItemPath(path => ItemPath.Text = path);
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;
            Keyboard.Focus(ItemPath);
        }

        private void Download([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var dialog = new SaveFileDialog
            {
                Title = Rocks.Resources.MediaManager_DownloadAttachment_Download,
                CheckPathExists = true,
                OverwritePrompt = true,
                FileName = ItemName + ".png",
                Filter = @"PNG Files|*.png|All files|*.*"
            };

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            using (var client = new WebClient())
            {
                client.DownloadFile(DatabaseUri.Site.GetHost() + FileName, dialog.FileName);
            }
        }

        private void EnableButtons()
        {
            TakeScreenshotButton.IsEnabled = !string.IsNullOrEmpty(ItemPath.Text);
            OkButton.IsEnabled = !string.IsNullOrEmpty(FileName);
            DownloadButton.IsEnabled = !string.IsNullOrEmpty(FileName);
        }

        private void EnableButtons([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
        }

        private void HandleMouseDown([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            IsMouseDown = true;
            Position = PointToScreen(e.GetPosition(this));
            Offset = new System.Windows.Point(RubberBand.Margin.Left, RubberBand.Margin.Top);

            Mouse.Capture(RubberBand, CaptureMode.Element);
            e.Handled = true;
        }

        private void HandleMouseMove([NotNull] object sender, [NotNull] System.Windows.Input.MouseEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!IsMouseDown)
            {
                return;
            }

            e.Handled = true;

            var position = PointToScreen(e.GetPosition(this));
            var x = Offset.X + position.X - Position.X;
            var y = Offset.Y + position.Y - Position.Y;

            if (x + 128 > Preview.ActualWidth)
            {
                x = Preview.ActualWidth - 128;
            }

            if (x < 0)
            {
                x = 0;
            }

            if (y + 128 > Preview.ActualHeight)
            {
                y = Preview.ActualHeight - 128;
            }

            if (y < 0)
            {
                y = 0;
            }

            RubberBand.Margin = new Thickness(x, y, 0, 0);

            X = (int)x;
            Y = (int)y;

            /*
      if (this.X + 128 > PreviewScrollViewer.HorizontalOffset + PreviewScrollViewer.ViewportWidth)
      {
        PreviewScrollViewer.ScrollToHorizontalOffset(this.X + 128 - PreviewScrollViewer.ViewportWidth);
      }

      if (this.X < PreviewScrollViewer.HorizontalOffset)
      {
        PreviewScrollViewer.ScrollToHorizontalOffset(this.X);
      }

      if (this.Y + 128 > PreviewScrollViewer.VerticalOffset + PreviewScrollViewer.ViewportHeight)
      {
        PreviewScrollViewer.ScrollToVerticalOffset(this.Y + 128 - PreviewScrollViewer.ViewportHeight);
      }

      if (this.Y < PreviewScrollViewer.VerticalOffset)
      {
        PreviewScrollViewer.ScrollToVerticalOffset(this.Y);
      }
      */
            UpdateThumbnails();
        }

        private void HandleMouseUp([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            IsMouseDown = false;
            Mouse.Capture(null);
            e.Handled = true;
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }

        private void TakeScreenshot([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var databaseName = DatabaseUri.DatabaseName.ToString();
            var source = ItemPath.Text ?? string.Empty;

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                TakeScreenshotButton.IsEnabled = true;

                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                if (string.IsNullOrEmpty(response))
                {
                    AppHost.MessageBox("Failed to take screenshot.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                FileName = response;
                UpdateSourceImage();
                UpdateThumbnails();
                RubberBand.Visibility = Visibility.Visible;
                EnableButtons();
            };

            Clear();
            FileName = string.Empty;
            EnableButtons();
            TakeScreenshotButton.IsEnabled = false;

            DatabaseUri.Site.DataService.ExecuteAsync("Items.GetThumbnail", completed, databaseName, source);
        }

        private void Transform([NotNull] System.Windows.Controls.Image image, double size)
        {
            Debug.ArgumentNotNull(image, nameof(image));

            var transform = new TransformedBitmap();
            transform.BeginInit();
            transform.Source = SourceImage;
            transform.Transform = new ScaleTransform(size / 128, size / 128);
            transform.EndInit();

            image.Source = transform;
        }

        private void UpdateSourceImage()
        {
            var path = DatabaseUri.Site.GetHost() + FileName;

            SourceImage = new BitmapImage();
            SourceImage.LoadIgnoreCache(new Uri(path));

            Preview.Source = SourceImage;

            Transform(Preview128x128, 128);
            Transform(Preview48x48, 48);
            Transform(Preview32x32, 32);
            Transform(Preview24x24, 24);
            Transform(Preview16x16, 16);
        }

        private void UpdateThumbnails()
        {
            Preview128x128.Margin = new Thickness(-X, -Y, 0, 0);

            Preview48x48.Margin = new Thickness((int)(-X * 48 / 128), (int)(-Y * 48 / 128), 0, 0);
            Preview32x32.Margin = new Thickness((int)(-X * 32 / 128), (int)(-Y * 32 / 128), 0, 0);
            Preview24x24.Margin = new Thickness((int)(-X * 24 / 128), (int)(-Y * 24 / 128), 0, 0);
            Preview16x16.Margin = new Thickness((int)(-X * 16 / 128), (int)(-Y * 16 / 128), 0, 0);
        }
    }
}
