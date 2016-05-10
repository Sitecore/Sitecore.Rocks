// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Media.Skins.Details
{
    [MediaSkin("Details", 600)]
    public partial class Details : IMediaSkin
    {
        private readonly ListViewSorter listViewSorter;

        private Point dragOrigin;

        public Details()
        {
            InitializeComponent();

            listViewSorter = new ListViewSorter(Hits);
        }

        [NotNull]
        public MediaViewer MediaViewer { get; set; }

        public Site Site { get; set; }

        public void Clear()
        {
            Hits.ItemsSource = null;
        }

        public void Deleted(ItemHeader item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            var itemsSource = Hits.ItemsSource as ListCollectionView;
            if (itemsSource != null)
            {
                itemsSource.Remove(item);
            }
        }

        public Control GetControl()
        {
            return this;
        }

        public IEnumerable<ItemHeader> GetSelectedItems()
        {
            foreach (var selectedItem in Hits.SelectedItems)
            {
                var itemHeader = selectedItem as ItemHeader;
                if (itemHeader == null)
                {
                    continue;
                }

                yield return itemHeader;
            }
        }

        public void Initialize(MediaViewer mediaViewer)
        {
            Assert.ArgumentNotNull(mediaViewer, nameof(mediaViewer));

            MediaViewer = mediaViewer;
        }

        public void Load(List<ItemHeader> items)
        {
            Assert.ArgumentNotNull(items, nameof(items));

            Hits.ItemsSource = new ListCollectionView(items);

            listViewSorter.Resort();
        }

        public void Renamed([NotNull] ItemHeader item, string newName)
        {
            Assert.ArgumentNotNull(item, nameof(item));
            Assert.ArgumentNotNull(newName, nameof(newName));

            var itemsSource = Hits.ItemsSource as ListCollectionView;
            if (itemsSource != null)
            {
                itemsSource.Refresh();
            }
        }

        private void HandleMouseLeftButtonDown([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            DragManager.HandleMouseDown(this, e, out dragOrigin);

            if (e.ClickCount < 2)
            {
                return;
            }

            var selectedItem = Hits.SelectedItem as ItemHeader;
            if (selectedItem == null)
            {
                return;
            }

            var itemVersionUri = new ItemVersionUri(selectedItem.ItemUri, LanguageManager.CurrentLanguage, Version.Latest);
            AppHost.OpenContentEditor(itemVersionUri);
        }

        private void HandleMouseMove([NotNull] object sender, [NotNull] MouseEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            if (!DragManager.IsDragStart(this, e, ref dragOrigin))
            {
                return;
            }

            var selectedItem = Hits.SelectedItem as ItemHeader;
            if (selectedItem == null)
            {
                return;
            }

            var list = new List<ItemHeader>
            {
                selectedItem
            };

            var dragData = DragManager.SetData(list);

            DragManager.DoDragDrop(this, dragData, DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);

            e.Handled = true;
        }

        private void HeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            listViewSorter.HeaderClick(sender, e);
        }
    }
}
