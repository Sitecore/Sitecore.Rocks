// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Media.Skins
{
    public partial class MediaSkinListBox
    {
        private Point dragOrigin;

        public MediaSkinListBox()
        {
            InitializeComponent();
        }

        public Site Site { get; set; }

        [NotNull]
        protected Func<ItemHeader, UserControl> GetHeader { get; set; }

        protected IMediaSkin Skin { get; set; }

        public void Clear()
        {
            ListBox.Items.Clear();
        }

        public void Deleted([NotNull] ItemHeader itemHeader)
        {
            Assert.ArgumentNotNull(itemHeader, nameof(itemHeader));

            for (var index = ListBox.Items.Count - 1; index >= 0; index--)
            {
                var listBoxItem = ListBox.Items[index] as ListBoxItem;
                if (listBoxItem == null)
                {
                    continue;
                }

                var h = listBoxItem.Tag as ItemHeader;
                if (h == null)
                {
                    continue;
                }

                if (h.ItemUri != itemHeader.ItemUri)
                {
                    continue;
                }

                ListBox.Items.Remove(listBoxItem);
            }
        }

        [NotNull]
        public IEnumerable<ItemHeader> GetSelectedItems()
        {
            foreach (var selectedItem in ListBox.SelectedItems)
            {
                var listBoxItem = selectedItem as ListBoxItem;
                if (listBoxItem == null)
                {
                    continue;
                }

                var itemHeader = listBoxItem.Tag as ItemHeader;
                if (itemHeader == null)
                {
                    continue;
                }

                yield return itemHeader;
            }
        }

        public void Initialize([NotNull] IMediaSkin skin, [NotNull] Func<ItemHeader, UserControl> getHeader)
        {
            Assert.ArgumentNotNull(skin, nameof(skin));
            Assert.ArgumentNotNull(getHeader, nameof(getHeader));

            Skin = skin;
            GetHeader = getHeader;
        }

        public void Renamed([NotNull] ItemHeader itemHeader, [NotNull] string newName)
        {
            Assert.ArgumentNotNull(itemHeader, nameof(itemHeader));
            Assert.ArgumentNotNull(newName, nameof(newName));

            for (var index = ListBox.Items.Count - 1; index >= 0; index--)
            {
                var listBoxItem = ListBox.Items[index] as ListBoxItem;
                if (listBoxItem == null)
                {
                    continue;
                }

                var h = listBoxItem.Tag as ItemHeader;
                if (h == null)
                {
                    continue;
                }

                if (h.ItemUri != itemHeader.ItemUri)
                {
                    continue;
                }

                listBoxItem.Content = newName;
            }
        }

        public void RenderItems([NotNull] IEnumerable<ItemHeader> items)
        {
            Assert.ArgumentNotNull(items, nameof(items));

            foreach (var itemHeader in items)
            {
                RenderItem(itemHeader);
            }
        }

        private void HandleDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = ListBox.SelectedItem as ListBoxItem;
            if (selectedItem == null)
            {
                return;
            }

            var itemHeader = selectedItem.Tag as ItemHeader;
            if (itemHeader == null)
            {
                return;
            }

            var itemVersionUri = new ItemVersionUri(itemHeader.ItemUri, LanguageManager.CurrentLanguage, Data.Version.Latest);

            AppHost.OpenContentEditor(itemVersionUri);
        }

        private void HandleDragOver([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void HandleDropFiles([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            var site = Site;
            if (site == null)
            {
                AppHost.MessageBox(Rocks.Resources.Search_HandleKeyDown_Please_select_a_database_first_, Rocks.Resources.Search_HandleKeyDown_Select_Database, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if ((site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Execute) != DataServiceFeatureCapabilities.Execute)
            {
                AppHost.MessageBox(string.Format(Rocks.Resources.MediaSkinListBox_HandleDropFiles_, site.DataServiceName), Rocks.Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var droppedFilePaths = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            if (droppedFilePaths == null)
            {
                return;
            }

            var databaseUri = new DatabaseUri(site, DatabaseName.Master);

            MediaManager.Upload(databaseUri, @"/sitecore/media library", droppedFilePaths, UploadCompleted);

            e.Handled = true;
        }

        private void HandleMouseLeftButtonDown([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            DragManager.HandleMouseDown(this, e, out dragOrigin);
        }

        private void HandleMouseMove([NotNull] object sender, [NotNull] MouseEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!DragManager.IsDragStart(this, e, ref dragOrigin))
            {
                return;
            }

            var selectedItems = GetSelectedItems();
            if (!selectedItems.Any())
            {
                return;
            }

            var dragData = DragManager.SetData(selectedItems);

            DragManager.DoDragDrop(this, dragData, DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);

            e.Handled = true;
        }

        [NotNull]
        private ListBoxItem RenderItem([NotNull] ItemHeader itemHeader)
        {
            Debug.ArgumentNotNull(itemHeader, nameof(itemHeader));

            var header = GetHeader(itemHeader);

            var listBoxItem = new ListBoxItem
            {
                Content = header,
                Tag = itemHeader
            };

            // listBoxItem.ToolTipOpening += delegate { listBoxItem.ToolTip = ToolTipBuilder.BuildToolTip(itemHeader); };
            ListBox.Items.Add(listBoxItem);

            return listBoxItem;
        }

        private void UploadCompleted([NotNull] ItemHeader itemHeader)
        {
            Debug.ArgumentNotNull(itemHeader, nameof(itemHeader));

            Skin.MediaViewer.AddItem(itemHeader);

            var listBoxItem = RenderItem(itemHeader);
            listBoxItem.IsSelected = true;
        }
    }
}
