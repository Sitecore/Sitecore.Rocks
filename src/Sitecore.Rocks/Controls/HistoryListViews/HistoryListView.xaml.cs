// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DateTimeExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Controls.HistoryListViews
{
    public partial class HistoryListView
    {
        private readonly ListViewSorter _listViewSorter;

        public HistoryListView()
        {
            InitializeComponent();

            _listViewSorter = new ListViewSorter(ListView);
            FilterText = string.Empty;

            Loaded += ControlLoaded;
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
        public Site Site { get; set; }

        [NotNull]
        protected string FilterText { get; set; }

        [CanBeNull]
        protected IEnumerable<ItemDescriptor> ItemDescriptors { get; set; }

        private bool IsLoading { get; set; }

        public void Refresh()
        {
            if (IsLoading)
            {
                return;
            }

            Loading.ShowLoading(ListView);

            ExecuteCompleted completed = delegate(string response, ExecuteResult executeresult)
            {
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

                ParseItems(root, Site);

                RenderItems();
            };

            var fromDate = DateTimeExtensions.ToIsoDate(FromDateTimeBox.Value ?? DateTime.UtcNow);
            var toDate = DateTimeExtensions.ToIsoDate(ToDateTimeBox.Value ?? DateTime.UtcNow);

            Site.DataService.ExecuteAsync("Items.GetModifiedItems", completed, string.Empty, fromDate, toDate);
        }

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

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            IsLoading = true;
            try
            {
                FromDateTimeBox.Value = DateTime.UtcNow.AddDays(-1);
                ToDateTimeBox.Value = DateTime.UtcNow;
            }
            finally
            {
                IsLoading = false;
            }

            Refresh();
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

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = new HistoryListViewContext(this, SelectedItems);

            ContextMenuPanel.ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void ParseItems([NotNull] XElement root, [NotNull] Site site)
        {
            Debug.ArgumentNotNull(root, nameof(root));
            Debug.ArgumentNotNull(site, nameof(site));

            Func<XElement, ItemDescriptor> selector = delegate(XElement element)
            {
                var databaseUri = new DatabaseUri(site, new DatabaseName(element.GetAttributeValue("database")));

                return new ItemDescriptor(ItemHeader.Parse(databaseUri, element));
            };

            ItemDescriptors = root.Elements().Select(selector).ToList();
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

        private void ValueChanged([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Refresh();
        }

        public class ItemDescriptor : DependencyObject
        {
            public ItemDescriptor([NotNull] ItemHeader item)
            {
                Assert.ArgumentNotNull(item, nameof(item));

                Item = item;
                IsChecked = true;
            }

            [NotNull]
            public string DatabaseName
            {
                get { return Item.ItemUri.DatabaseName.ToString(); }
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
