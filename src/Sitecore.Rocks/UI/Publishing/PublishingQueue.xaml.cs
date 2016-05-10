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
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Searching;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Publishing
{
    public partial class PublishingQueue : IContextProvider
    {
        private readonly List<ItemHeader> entries = new List<ItemHeader>();

        private readonly ListViewSorter listViewSorter;

        private TreeViewItem pathsAllTreeViewItem;

        private TreeViewItem updatedAllTreeViewItem;

        public PublishingQueue()
        {
            InitializeComponent();

            listViewSorter = new ListViewSorter(Items);

            UpdatedByLabel.MouseDown += (sender, args) => UpdatedBy.SelectedIndex = 0;
            PathLabel.MouseDown += (sender, args) => pathsAllTreeViewItem.IsSelected = true;
            UpdatedLabel.MouseDown += (sender, args) => updatedAllTreeViewItem.IsSelected = true;
        }

        [NotNull]
        public DatabaseUri DatabaseUri { get; private set; }

        public int PageNumber { get; private set; }

        [NotNull]
        public IPane Pane { get; set; }

        public void Clear()
        {
            entries.Clear();
            PageNumber = 0;
            Items.ItemsSource = null;
            UpdatedBy.Items.Clear();
            Paths.Items.Clear();
            Updated.Items.Clear();
        }

        [NotNull]
        public object GetContext()
        {
            return new PublishingQueueContext(this)
            {
                SelectedItems = Items.SelectedItems.OfType<ItemHeader>()
            };
        }

        public void GetPublishingCandidates(int pageNumber)
        {
            var site = DatabaseUri.Site;
            if (site == Site.Empty)
            {
                AppHost.MessageBox(Rocks.Resources.Search_HandleKeyDown_Please_select_a_database_first_, Rocks.Resources.Search_HandleKeyDown_Select_Database, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ExecuteCompleted c = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    Loading.Visibility = Visibility.Collapsed;
                    List.Visibility = Visibility.Visible;
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    Loading.Visibility = Visibility.Collapsed;
                    List.Visibility = Visibility.Visible;
                    return;
                }

                var list = new List<ItemHeader>();

                foreach (var element in root.Elements())
                {
                    var itemHeader = ItemHeader.Parse(DatabaseUri, element);
                    list.Add(itemHeader);
                }

                LoadItems(list);
            };

            Loading.Visibility = Visibility.Visible;
            List.Visibility = Visibility.Collapsed;

            DatabaseUri.Site.DataService.ExecuteAsync("Publishing.GetPublishingQueue", c, DatabaseUri.DatabaseName.ToString(), pageNumber);
        }

        public void Initialize([NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            SetDatabaseUri(databaseUri);
            Pane.Caption = string.Format(@"Publish Queue [{0}/{1}]", DatabaseUri.DatabaseName, DatabaseUri.Site.Name);
        }

        public void NextPage()
        {
            PageNumber++;

            GetPublishingCandidates(PageNumber);
        }

        public void SetDatabaseUri([NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            DatabaseUri = databaseUri;
            Clear();

            Pane.Caption = string.Format(@"Publish Queue [{0}/{1}]", DatabaseUri.DatabaseName, DatabaseUri.Site.Name);

            GetPublishingCandidates(0);
        }

        public void ToggleFilters()
        {
            if (FilterHeight.Height.Value == 0)
            {
                var height = 100;
                var value = AppHost.Settings.Get("PublishingQueue", "GridSplitterPosition", "100");
                if (value != null)
                {
                    int.TryParse(value as string, out height);
                }

                FilterHeight.Height = new GridLength(height);
            }
            else
            {
                FilterHeight.Height = new GridLength(0);
            }
        }

        private void ApplyFilters([NotNull] List<ItemHeader> list)
        {
            Debug.ArgumentNotNull(list, nameof(list));

            var authors = new List<string>();
            var path = string.Empty;
            var year = 0;
            var month = 0;
            var day = 0;

            GetFilter(UpdatedBy.Items, authors);

            var selectedItem = Paths.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                path = selectedItem.Tag as string ?? string.Empty;
            }

            selectedItem = Updated.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                var value = selectedItem.Tag as string ?? string.Empty;

                if (!string.IsNullOrEmpty(value))
                {
                    var parts = value.Split('/');
                    if (parts.Length > 0)
                    {
                        year = int.Parse(parts[0]);
                    }

                    if (parts.Length > 1)
                    {
                        month = int.Parse(parts[1]);
                    }

                    if (parts.Length > 2)
                    {
                        day = int.Parse(parts[2]);
                    }
                }
            }

            list.AddRange(entries.Where(entry => GetVisible(entry, authors, path, year, month, day)));
        }

        private void ApplyFilters([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            LoadItems();
        }

        private void ApplyFilters([NotNull] object sender, [NotNull] RoutedPropertyChangedEventArgs<object> e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            LoadItems();
        }

        private bool Contains([NotNull] List<string> items, [NotNull] string value)
        {
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(value, nameof(value));

            return items.Any(s => string.Compare(s, value, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        private void GetFilter([NotNull] ItemCollection items, [NotNull] List<string> selecteditems)
        {
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(selecteditems, nameof(selecteditems));

            foreach (var item in items)
            {
                var listBoxItem = item as ListBoxItem;
                if (listBoxItem == null)
                {
                    continue;
                }

                if (!listBoxItem.IsSelected)
                {
                    continue;
                }

                var value = listBoxItem.Tag as string ?? string.Empty;
                if (!string.IsNullOrEmpty(value))
                {
                    selecteditems.Add(value);
                }
            }
        }

        [NotNull]
        private TreeViewItem GetTreeViewItem([NotNull] ItemCollection items, [NotNull] Tuple<string, string> value)
        {
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(value, nameof(value));

            var index = -1;

            for (var i = 0; i < items.Count; i++)
            {
                var treeViewItem = items[i] as TreeViewItem;
                if (treeViewItem == null)
                {
                    continue;
                }

                var v = treeViewItem.Tag as string ?? string.Empty;

                var compare = string.Compare(v, value.Item1, StringComparison.InvariantCultureIgnoreCase);
                if (compare == 0)
                {
                    var header = (SearchHeader)treeViewItem.Header;
                    header.ItemCount++;
                    return treeViewItem;
                }

                if (compare > 0)
                {
                    index = i;
                    break;
                }
            }

            var item = new TreeViewItem
            {
                Header = new SearchHeader
                {
                    Text = value.Item2,
                    ItemCount = 1
                },
                Tag = value.Item1
            };

            if (index < 0)
            {
                items.Add(item);
            }
            else
            {
                items.Insert(index, item);
            }

            return item;
        }

        private bool GetVisible([NotNull] ItemHeader item, [NotNull] List<string> updatedBy, [NotNull] string path, int year, int month, int day)
        {
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(updatedBy, nameof(updatedBy));
            Debug.ArgumentNotNull(path, nameof(path));

            if (updatedBy.Count > 0)
            {
                if (!Contains(updatedBy, item.UpdatedBy))
                {
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(path))
            {
                if (!item.Path.StartsWith(path, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
            }

            if (year != 0)
            {
                if (item.Updated.Year != year)
                {
                    return false;
                }
            }

            if (month != 0)
            {
                if (item.Updated.Month != month)
                {
                    return false;
                }
            }

            if (day != 0)
            {
                if (item.Updated.Day != day)
                {
                    return false;
                }
            }

            return true;
        }

        private void HeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            listViewSorter.HeaderClick(sender, e);
        }

        private void LoadFilters([NotNull] IEnumerable<ItemHeader> items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            foreach (var item in items)
            {
                LoadFilters(item);
            }
        }

        private void LoadFilters([NotNull] ItemHeader item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            LoadFiltersAll();

            LoadFilterValue(UpdatedBy.Items, new Tuple<string, string>(item.UpdatedBy, item.UpdatedBy));

            var path = string.Empty;
            var parts = new List<Tuple<string, string>>();
            foreach (var s in item.Path.Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                if (string.IsNullOrEmpty(s))
                {
                    continue;
                }

                path += @"/" + s;
                parts.Add(new Tuple<string, string>(path, s));
            }

            LoadFilterTree(Paths.Items, parts, 0);

            if (item.Updated == DateTime.MinValue)
            {
                return;
            }

            var year = item.Updated.Year.ToString();
            var month = item.Updated.Month.ToString();
            var day = item.Updated.Day.ToString();

            var tuples = new List<Tuple<string, string>>
            {
                new Tuple<string, string>(year, year),
                new Tuple<string, string>(year + @"/" + month, item.Updated.ToString(@"MMMM")),
                new Tuple<string, string>(year + @"/" + month + @"/" + day, day)
            };

            LoadFilterTree(Updated.Items, tuples, 0);
        }

        private void LoadFiltersAll()
        {
            if (UpdatedBy.Items.Count == 0)
            {
                UpdatedBy.Items.Add(new ListBoxItem
                {
                    Content = Rocks.Resources.Search_LoadFilters__all_
                });
            }

            if (Paths.Items.Count == 0)
            {
                pathsAllTreeViewItem = new TreeViewItem
                {
                    Header = Rocks.Resources.Search_LoadFilters__all_
                };

                Paths.Items.Add(pathsAllTreeViewItem);
            }

            if (Updated.Items.Count == 0)
            {
                updatedAllTreeViewItem = new TreeViewItem
                {
                    Header = Rocks.Resources.Search_LoadFilters__all_
                };

                Updated.Items.Add(updatedAllTreeViewItem);
            }
        }

        private void LoadFilterTree([NotNull] ItemCollection items, [NotNull] List<Tuple<string, string>> path, int index)
        {
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(path, nameof(path));

            var treeViewItem = GetTreeViewItem(items, path[index]);

            if (index < path.Count - 1)
            {
                LoadFilterTree(treeViewItem.Items, path, index + 1);
            }
        }

        private void LoadFilterValue([NotNull] ItemCollection items, [NotNull] Tuple<string, string> value)
        {
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(value, nameof(value));

            if (string.IsNullOrEmpty(value.Item1))
            {
                return;
            }

            var index = -1;

            for (var i = 0; i < items.Count; i++)
            {
                var listBoxItem = items[i] as ListBoxItem;
                if (listBoxItem == null)
                {
                    continue;
                }

                var v = listBoxItem.Tag as string ?? string.Empty;
                var compare = string.Compare(v, value.Item1, StringComparison.InvariantCultureIgnoreCase);
                if (compare == 0)
                {
                    var header = (SearchHeader)listBoxItem.Content;
                    header.ItemCount++;
                    return;
                }

                if (compare > 0)
                {
                    index = i;
                    break;
                }
            }

            var item = new ListBoxItem
            {
                Content = new SearchHeader
                {
                    Text = value.Item2,
                    ItemCount = 1
                },
                Tag = value.Item1
            };

            if (index < 0)
            {
                items.Add(item);
            }
            else
            {
                items.Insert(index, item);
            }
        }

        private void LoadItems()
        {
            var list = new List<ItemHeader>();

            ApplyFilters(list);

            Items.ItemsSource = list;

            listViewSorter.Resort();
        }

        private void LoadItems([NotNull] IEnumerable<ItemHeader> items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            LoadFilters(items);

            entries.AddRange(items);

            LoadItems();

            Loading.Visibility = Visibility.Collapsed;
            List.Visibility = Visibility.Visible;
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = GetContext();

            ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void SaveGridSplitterPosition([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (FilterHeight.Height.Value > 0)
            {
                AppHost.Settings.Set("PublishingQueue", "GridSplitterPosition", FilterHeight.Height.Value.ToString());
            }
        }

        private void ToggleFilters([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ToggleFilters();
        }
    }
}
