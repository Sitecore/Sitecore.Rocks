// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.Controls.ItemDependencyListViews
{
    public partial class ItemDependencyListView
    {
        private readonly ListViewSorter _listViewSorter;

        private IEnumerable<ItemUri> _itemsSource;

        public ItemDependencyListView()
        {
            InitializeComponent();

            _listViewSorter = new ListViewSorter(ListView);
            FilterText = string.Empty;
        }

        [CanBeNull]
        public object Header
        {
            get { return ListViewLabel.Content; }

            set { ListViewLabel.Content = value; }
        }

        [NotNull]
        public IEnumerable<ItemUri> ItemsSource
        {
            get { return _itemsSource ?? Enumerable.Empty<ItemUri>(); }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                _itemsSource = value;

                LoadItems();
            }
        }

        [NotNull]
        public IEnumerable<ItemDescriptor> SelectedItemDescriptors
        {
            get
            {
                var itemDescriptors = ItemDescriptors;
                if (itemDescriptors == null)
                {
                    return Enumerable.Empty<ItemDescriptor>();
                }

                return itemDescriptors.Where(i => i.IsChecked && i.ItemName.IsFilterMatch(FilterText));
            }
        }

        [NotNull]
        public IEnumerable<ItemUri> SelectedItems
        {
            get
            {
                var itemDescriptors = ItemDescriptors;
                if (itemDescriptors == null)
                {
                    return Enumerable.Empty<ItemUri>();
                }

                return itemDescriptors.Where(i => i.IsChecked && i.ItemName.IsFilterMatch(FilterText)).Select(i => i.Item.ItemUri);
            }
        }

        [NotNull]
        protected string FilterText { get; set; }

        [CanBeNull]
        protected IEnumerable<ItemDescriptor> ItemDescriptors { get; set; }

        public void RefreshItems()
        {
            ListView.Items.Refresh();
            RenderSelectedCount();
        }

        public void SetCheckBoxes([NotNull] Action<ItemDescriptor> action)
        {
            Assert.ArgumentNotNull(action, nameof(action));

            var itemDescriptors = ItemDescriptors;
            if (itemDescriptors == null)
            {
                return;
            }

            foreach (var itemDescriptor in itemDescriptors.Where(i => i.ItemName.IsFilterMatch(FilterText)))
            {
                action(itemDescriptor);
            }
        }

        private void CheckAll([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            SetCheckBoxes(i => i.IsChecked = true);
            RefreshItems();
        }

        private void FilterChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            FilterText = Filter.Text;
            RenderItems();
        }

        private void HeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            _listViewSorter.HeaderClick(sender, e);
        }

        private void ItemChecked([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            RenderSelectedCount();
        }

        private void LoadItems()
        {
            Loading.ShowLoading(ListView);

            if (!ItemsSource.Any())
            {
                Loading.HideLoading(ListView);
                return;
            }

            var databaseUri = ItemsSource.First().DatabaseUri;

            var itemList = new StringBuilder();
            foreach (var itemUri in ItemsSource)
            {
                if (itemList.Length > 0)
                {
                    itemList.Append('|');
                }

                itemList.Append(itemUri.ItemId);
            }

            ExecuteCompleted completed = delegate(string response, ExecuteResult executeresult)
            {
                Debug.ArgumentNotNull(response, nameof(response));
                Debug.ArgumentNotNull(executeresult, nameof(executeresult));

                Loading.HideLoading(ListView);
                ListView.ItemsSource = null;

                if (!DataService.HandleExecute(response, executeresult))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                ParseItems(root, databaseUri);

                RenderItems();
            };

            databaseUri.Site.DataService.ExecuteAsync("Items.GetDependencies", completed, databaseUri.DatabaseName.Name, itemList.ToString());
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = new ItemDependencyContext(this, SelectedItems);

            ContextMenuPanel.ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void ParseItems([NotNull] XElement root, [NotNull] DatabaseUri databaseUri)
        {
            Debug.ArgumentNotNull(root, nameof(root));
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));

            var items = root.Elements().Select(element => ItemHeader.Parse(databaseUri, element)).ToList();

            ItemDescriptors = items.Select(item => new ItemDescriptor(item)).ToList();
        }

        private void RenderItems()
        {
            if (_listViewSorter == null)
            {
                return;
            }

            var itemDescriptors = ItemDescriptors;
            if (itemDescriptors == null)
            {
                return;
            }

            ListView.ItemsSource = null;
            ListView.ItemsSource = itemDescriptors.Where(i => i.ItemName.IsFilterMatch(FilterText));

            _listViewSorter.Resort();

            RenderSelectedCount();
        }

        private void RenderSelectedCount()
        {
            var selectCount = SelectCount;
            if (selectCount == null)
            {
                return;
            }

            var on = 0;
            var count = 0;

            SetCheckBoxes(item =>
            {
                if (item.IsChecked)
                {
                    on++;
                }

                count++;
            });

            selectCount.Text = string.Format("{0} of {1} selected", on, count);
        }

        private void UncheckAll([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            SetCheckBoxes(i => i.IsChecked = false);
            RefreshItems();
        }

        public class ItemDescriptor : DependencyObject
        {
            public ItemDescriptor([NotNull] ItemHeader item)
            {
                Assert.ArgumentNotNull(item, nameof(item));

                Item = item;
                IsChecked = true;
            }

            public bool IsChecked { get; set; }

            [NotNull]
            public ItemHeader Item { get; }

            [NotNull]
            public string ItemName
            {
                get { return Item.Name; }
            }

            [NotNull]
            public string Path
            {
                get { return Item.Path; }
            }

            [NotNull]
            public string TemplateName
            {
                get { return Item.TemplateName; }
            }

            public DateTime Updated
            {
                get { return Item.Updated; }
            }

            [NotNull]
            public string UpdatedBy
            {
                get { return Item.UpdatedBy; }
            }
        }
    }
}
