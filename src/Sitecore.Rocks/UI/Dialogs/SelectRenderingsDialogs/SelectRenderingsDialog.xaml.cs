// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.Dialogs.SelectRenderingsDialogs
{
    public partial class SelectRenderingsDialog
    {
        [UsedImplicitly]
        private ListBoxSorter listBoxSorter;

        public SelectRenderingsDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            listBoxSorter = new ListBoxSorter(SelectedRenderingsListBox);
        }

        [NotNull]
        public string HelpText
        {
            get { return DialogHelpText.Text; }

            set { DialogHelpText.Text = value; }
        }

        [NotNull]
        public string Label
        {
            get { return LabelTextBlock.Text; }

            set { LabelTextBlock.Text = value; }
        }

        [NotNull]
        public List<ItemId> SelectedRenderings
        {
            get { return SelectedRenderingsListBox.Items.OfType<ListBoxItem>().Select(listBoxItem => listBoxItem.Tag).OfType<ItemHeader>().Select(itemHeader => itemHeader.ItemId).ToList(); }
        }

        [NotNull]
        private List<ItemId> SelectedItems { get; set; }

        public void Initialize([NotNull] string title, [NotNull] DatabaseUri databaseUri, [NotNull] IEnumerable<ItemId> selectedRenderings)
        {
            Assert.ArgumentNotNull(title, nameof(title));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(selectedRenderings, nameof(selectedRenderings));

            Title = title;
            SelectedItems = new List<ItemId>(selectedRenderings);

            RenderingPicker.Initialize(databaseUri, selectedRenderings);
        }

        private void HandleKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key == Key.Delete)
            {
                RemoveSelected();
            }
        }

        private void MoveDown(object sender, RoutedEventArgs e)
        {
            for (var n = SelectedRenderingsListBox.Items.Count - 2; n >= 0; n--)
            {
                var item = SelectedRenderingsListBox.Items[n] as ListBoxItem;
                if (item == null)
                {
                    continue;
                }

                if (!item.IsSelected)
                {
                    continue;
                }

                SelectedRenderingsListBox.Items.RemoveAt(n);

                SelectedRenderingsListBox.Items.Insert(n + 1, item);

                item.IsSelected = true;
            }
        }

        private void MoveUp(object sender, RoutedEventArgs e)
        {
            for (var n = 1; n < SelectedRenderingsListBox.Items.Count; n++)
            {
                var item = SelectedRenderingsListBox.Items[n] as ListBoxItem;
                if (item == null)
                {
                    continue;
                }

                if (!item.IsSelected)
                {
                    continue;
                }

                SelectedRenderingsListBox.Items.RemoveAt(n);

                SelectedRenderingsListBox.Items.Insert(n - 1, item);

                item.IsSelected = true;
            }
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }

        private void OpenContextMenu(object sender, ContextMenuEventArgs e)
        {
            var selectedItem = SelectedRenderingsListBox.SelectedItem as ListBoxItem;
            if (selectedItem == null)
            {
                return;
            }

            var itemHeader = selectedItem.Tag as ItemHeader;
            if (itemHeader == null)
            {
                return;
            }

            var context = new ItemSelectionContext(new ItemDescriptor(itemHeader.ItemUri, itemHeader.Name));

            SelectedRenderingsBorder.ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void Refresh()
        {
            SelectedRenderingsListBox.Items.Clear();

            foreach (var selectedItem in SelectedItems)
            {
                var itemHeader = RenderingPicker.GetItemHeader(selectedItem);
                if (itemHeader == null)
                {
                    continue;
                }

                var listBoxItem = new ListBoxItem
                {
                    Tag = itemHeader,
                    Content = itemHeader.Name
                };

                SelectedRenderingsListBox.Items.Add(listBoxItem);
            }
        }

        private void Remove([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            RemoveSelected();
        }

        private void RemoveSelected()
        {
            var index = SelectedRenderingsListBox.SelectedIndex;

            var selectedItems = SelectedRenderingsListBox.SelectedItems.OfType<ListBoxItem>().Select(listBoxItem => listBoxItem.Tag).OfType<ItemHeader>().Select(itemHeader => itemHeader.ItemId).ToList();

            foreach (var selectedItem in selectedItems)
            {
                RenderingPicker.Remove(selectedItem);
            }

            // workaround for WPF quirk when checkbox is not visible, the Checked event does not fire
            SelectedItems = RenderingPicker.SelectedItems;
            Refresh();

            if (index >= SelectedRenderingsListBox.Items.Count)
            {
                index = SelectedRenderingsListBox.Items.Count - 1;
            }

            if (index >= 0)
            {
                SelectedRenderingsListBox.SelectedIndex = index;
            }
        }

        private void RenderingsLoaded([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Refresh();
        }

        private void SelectionChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            SelectedItems = RenderingPicker.SelectedItems;
            Refresh();
        }

        private void SetSelection([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = SelectedRenderingsListBox.SelectedItems.OfType<ListBoxItem>().Select(listBoxItem => listBoxItem.Tag).OfType<ItemHeader>().Select(itemHeader => itemHeader.ItemId).FirstOrDefault();
            if (selectedItem == null)
            {
                return;
            }

            RenderingPicker.FocusItem(selectedItem);
        }
    }
}
