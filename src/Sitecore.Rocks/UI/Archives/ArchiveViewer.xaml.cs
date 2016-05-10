// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Extensions.DateTimeExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Searching;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.UI.Archives
{
    public partial class ArchiveViewer : IContextProvider
    {
        private readonly List<ArchiveEntry> _entries = new List<ArchiveEntry>();

        private readonly ListViewSorter _listViewSorter;

        private TreeViewItem _pathsAllTreeViewItem;

        private TreeViewItem _updatedAllTreeViewItem;

        public ArchiveViewer()
        {
            InitializeComponent();

            _listViewSorter = new ListViewSorter(ArchiveEntries);

            ArchivedByLabel.MouseDown += (sender, args) => ArchivedBy.SelectedIndex = 0;
            PathLabel.MouseDown += (sender, args) => _pathsAllTreeViewItem.IsSelected = true;
            ArchivedLabel.MouseDown += (sender, args) => _updatedAllTreeViewItem.IsSelected = true;
        }

        public string ArchiveName { get; set; }

        public DatabaseUri DatabaseUri { get; private set; }

        public int PageNumber { get; private set; }

        public IPane Pane { get; set; }

        protected string Caption { get; set; }

        public void Clear()
        {
            _entries.Clear();
            PageNumber = 0;
            ArchiveEntries.ItemsSource = null;
            ArchivedBy.Items.Clear();
            Paths.Items.Clear();
            Archived.Items.Clear();
        }

        public void GetArchivedItems(int pageNumber)
        {
            var site = DatabaseUri.Site;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (site == null)
            {
                AppHost.MessageBox(Rocks.Resources.Search_HandleKeyDown_Please_select_a_database_first_, Rocks.Resources.Search_HandleKeyDown_Select_Database, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            site.DataService.GetArchivedItems(new DatabaseUri(site, DatabaseName.Master), ArchiveName, pageNumber, LoadItems);
        }

        [NotNull]
        public object GetContext()
        {
            return new ArchiveContext(this)
            {
                SelectedItems = ArchiveEntries.SelectedItems.OfType<ArchiveEntry>().ToList()
            };
        }

        public void Initialize([NotNull, Localizable(false)] string archiveName, [NotNull] string caption, [NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(archiveName, nameof(archiveName));
            Assert.ArgumentNotNull(caption, nameof(caption));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            ArchiveName = archiveName;
            Caption = caption;
            SetDatabaseUri(databaseUri);
        }

        public void NextPage()
        {
            PageNumber++;

            GetArchivedItems(PageNumber);
        }

        public void SetDatabaseUri([NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            DatabaseUri = databaseUri;
            Clear();

            Pane.Caption = $@"{Caption} - {DatabaseUri.DatabaseName} [{DatabaseUri.Site.Name}]";

            GetArchivedItems(0);
        }

        public void ToggleFilters()
        {
            if (FilterHeight.Height.Value == 0)
            {
                var height = 100;
                var value = AppHost.Settings.Get("Archives/" + ArchiveName.Capitalize(), "GridSplitterPosition", "100");
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

        private void ApplyFilters([NotNull] List<ArchiveEntry> list)
        {
            Debug.ArgumentNotNull(list, nameof(list));

            var authors = new List<string>();
            var path = string.Empty;
            var year = 0;
            var month = 0;
            var day = 0;

            GetFilter(ArchivedBy.Items, authors);

            var selectedItem = Paths.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                path = selectedItem.Tag as string ?? string.Empty;
            }

            selectedItem = Archived.SelectedItem as TreeViewItem;
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

            list.AddRange(_entries.Where(entry => GetVisible(entry, authors, path, year, month, day)));
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

        private bool GetVisible([NotNull] ArchiveEntry item, [NotNull] List<string> archivedBy, [NotNull] string path, int year, int month, int day)
        {
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(archivedBy, nameof(archivedBy));
            Debug.ArgumentNotNull(path, nameof(path));

            if (archivedBy.Count > 0)
            {
                if (!Contains(archivedBy, item.ArchivedBy))
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
                if (item.Archived.Year != year)
                {
                    return false;
                }
            }

            if (month != 0)
            {
                if (item.Archived.Month != month)
                {
                    return false;
                }
            }

            if (day != 0)
            {
                if (item.Archived.Day != day)
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

            _listViewSorter.HeaderClick(sender, e);
        }

        private void LoadFilters([NotNull] IEnumerable<ArchiveEntry> items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            foreach (var item in items)
            {
                LoadFilters(item);
            }
        }

        private void LoadFilters([NotNull] ArchiveEntry item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            LoadFiltersAll();

            LoadFilterValue(ArchivedBy.Items, new Tuple<string, string>(item.ArchivedBy, item.ArchivedBy));

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

            if (item.Archived == DateTime.MinValue)
            {
                return;
            }

            var year = item.Archived.Year.ToString();
            var month = item.Archived.Month.ToString();
            var day = item.Archived.Day.ToString();

            var tuples = new List<Tuple<string, string>>
            {
                new Tuple<string, string>(year, year),
                new Tuple<string, string>(year + @"/" + month, item.Archived.ToString(@"MMMM")),
                new Tuple<string, string>(year + @"/" + month + @"/" + day, day)
            };

            LoadFilterTree(Archived.Items, tuples, 0);
        }

        private void LoadFiltersAll()
        {
            if (ArchivedBy.Items.Count == 0)
            {
                ArchivedBy.Items.Add(new ListBoxItem
                {
                    Content = Rocks.Resources.Search_LoadFilters__all_
                });
            }

            if (Paths.Items.Count == 0)
            {
                _pathsAllTreeViewItem = new TreeViewItem
                {
                    Header = Rocks.Resources.Search_LoadFilters__all_
                };

                Paths.Items.Add(_pathsAllTreeViewItem);
            }

            if (Archived.Items.Count == 0)
            {
                _updatedAllTreeViewItem = new TreeViewItem
                {
                    Header = Rocks.Resources.Search_LoadFilters__all_
                };

                Archived.Items.Add(_updatedAllTreeViewItem);
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
            var list = new List<ArchiveEntry>();

            ApplyFilters(list);

            ArchiveEntries.ItemsSource = list;

            _listViewSorter.Resort();
        }

        private void LoadItems([NotNull] XDocument document)
        {
            Debug.ArgumentNotNull(document, nameof(document));

            var root = document.Root;
            if (root == null)
            {
                return;
            }

            var items = new List<ArchiveEntry>();

            foreach (var element in root.Elements())
            {
                var entry = new ArchiveEntry
                {
                    Id = new Guid(element.GetAttributeValue("id")),
                    Name = element.GetAttributeValue("name"),
                    Archived = DateTimeExtensions.FromIso(element.GetAttributeValue("datetime")),
                    ArchivedBy = element.GetAttributeValue("archivedby"),
                    Path = element.GetAttributeValue("path")
                };

                items.Add(entry);
            }

            LoadFilters(items);

            _entries.AddRange(items);

            LoadItems();
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
                AppHost.Settings.Set("Archives/" + ArchiveName.Capitalize(), "GridSplitterPosition", FilterHeight.Height.Value.ToString(CultureInfo.InvariantCulture));
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
