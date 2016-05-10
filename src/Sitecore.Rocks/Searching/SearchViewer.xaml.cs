// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ContextMenuExtensions;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Searching.Commands;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Searching
{
    public partial class SearchViewer : IContextProvider
    {
        private readonly List<ItemHeader> hits = new List<ItemHeader>();

        private readonly ListViewSorter listViewSorter;

        private Point dragOrigin;

        private TreeViewItem pathsAllTreeViewItem;

        private TreeViewItem updatedAllTreeViewItem;

        public SearchViewer()
        {
            InitializeComponent();

            listViewSorter = new ListViewSorter(Hits);

            TemplateLabel.MouseDown += (sender, args) => Templates.SelectedIndex = 0;
            AuthorLabel.MouseDown += (sender, args) => Authors.SelectedIndex = 0;
            CategoryLabel.MouseDown += (sender, args) => Categories.SelectedIndex = 0;
            PathLabel.MouseDown += delegate
            {
                var item = pathsAllTreeViewItem;
                if (item != null)
                {
                    item.IsSelected = true;
                }
            };
            UpdatedLabel.MouseDown += delegate
            {
                var item = updatedAllTreeViewItem;
                if (item != null)
                {
                    item.IsSelected = true;
                }
            };

            Notifications.RegisterItemEvents(this, deleted: ItemDeleted, renamed: ItemRenamed);
        }

        [NotNull]
        public DatabaseUri DatabaseUri
        {
            get { return new DatabaseUri(Site, DatabaseName.Master); }
        }

        public string LastField { get; private set; }

        public string LastQueryText { get; private set; }

        public ItemUri LastRootUri { get; set; }

        public int PageNumber { get; private set; }

        public IPane Pane { get; set; }

        public Site Site { get; private set; }

        [NotNull]
        public object GetContext()
        {
            var selectedItems = new List<ItemHeader>();

            foreach (var item in Hits.SelectedItems)
            {
                var itemHeader = item as ItemHeader;
                if (itemHeader != null)
                {
                    selectedItems.Add(itemHeader);
                }
            }

            return new SearchContext(this)
            {
                SelectedItems = selectedItems
            };
        }

        public void Initialize([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            SetSite(site);

            LastRootUri = ItemUri.Empty;
        }

        public void NextPage()
        {
            PageNumber++;

            Query(LastQueryText, LastField, LastRootUri, PageNumber);
        }

        public void Query([NotNull] string queryText, [NotNull] string field, [NotNull] ItemUri rootUri, int pageNumber)
        {
            Assert.ArgumentNotNull(queryText, nameof(queryText));
            Assert.ArgumentNotNull(field, nameof(field));
            Assert.ArgumentNotNull(rootUri, nameof(rootUri));

            Site.DataService.Search(queryText, new DatabaseUri(Site, DatabaseName.Master), field, string.Empty, rootUri, pageNumber, LoadHits);
        }

        public void Query()
        {
            Query(ItemUri.Empty);
        }

        public void ResetFilters()
        {
            Templates.SelectedIndex = 0;
            Authors.SelectedIndex = 0;
            Categories.SelectedIndex = 0;

            if (pathsAllTreeViewItem != null)
            {
                pathsAllTreeViewItem.IsSelected = true;
            }

            if (updatedAllTreeViewItem != null)
            {
                updatedAllTreeViewItem.IsSelected = true;
            }
        }

        public void Search([NotNull] string value)
        {
            Assert.ArgumentNotNull(value, nameof(value));

            Field.SelectedIndex = 0;
            SearchText.Text = value;

            Query();
        }

        public void Search([Localizable(false), NotNull] string fieldName, [NotNull] string value)
        {
            Assert.ArgumentNotNull(fieldName, nameof(fieldName));
            Assert.ArgumentNotNull(value, nameof(value));

            SearchText.Text = value;
            SetFieldName(fieldName);

            Query();
        }

        public void Search([Localizable(false), NotNull] string fieldName, [NotNull] ItemUri rootUri, [NotNull] string value)
        {
            Assert.ArgumentNotNull(fieldName, nameof(fieldName));
            Assert.ArgumentNotNull(rootUri, nameof(rootUri));
            Assert.ArgumentNotNull(value, nameof(value));

            SearchText.Text = value;
            SetFieldName(fieldName);

            Query(rootUri);
        }

        public void SetSite([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            Site = site;
            Pane.Caption = @"Search in Sitecore [" + site.Name + "]";
            Clear();
        }

        private void ApplyFilters([NotNull] List<ItemHeader> list)
        {
            Debug.ArgumentNotNull(list, nameof(list));

            var templates = new List<string>();
            var authors = new List<string>();
            var categories = new List<string>();
            var path = string.Empty;
            var year = 0;
            var month = 0;
            var day = 0;

            GetFilter(Templates.Items, templates);
            GetFilter(Authors.Items, authors);
            GetFilter(Categories.Items, categories);

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

            list.AddRange(hits.Where(item => GetVisible(item, templates, authors, categories, path, year, month, day)));
        }

        private void ApplyFilters([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            LoadHits();
        }

        private void ApplyFilters([NotNull] object sender, [NotNull] RoutedPropertyChangedEventArgs<object> e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            LoadHits();
        }

        private void Clear()
        {
            hits.Clear();
            PageNumber = 0;
            Hits.ItemsSource = null;
            Templates.Items.Clear();
            Authors.Items.Clear();
            Categories.Items.Clear();
            Paths.Items.Clear();
            Updated.Items.Clear();
        }

        private bool Contains([NotNull] List<string> items, [NotNull] string value)
        {
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(value, nameof(value));

            return items.Any(template => string.Compare(template, value, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        private bool ContainsGuid([NotNull] string expression)
        {
            Debug.ArgumentNotNull(expression, nameof(expression));

            var guidRegEx = new Regex(@"(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})");

            return guidRegEx.IsMatch(expression);
        }

        private void FiltersClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ToggleFilters();
        }

        private void GetFilter([NotNull] ItemCollection items, [NotNull] List<string> selectedItems)
        {
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(selectedItems, nameof(selectedItems));

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
                    selectedItems.Add(value);
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

        private bool GetVisible([NotNull] ItemHeader item, [NotNull] List<string> templates, [NotNull] List<string> authors, [NotNull] List<string> categories, [NotNull] string path, int year, int month, int day)
        {
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(templates, nameof(templates));
            Debug.ArgumentNotNull(authors, nameof(authors));
            Debug.ArgumentNotNull(categories, nameof(categories));
            Debug.ArgumentNotNull(path, nameof(path));

            if (templates.Count > 0)
            {
                if (!Contains(templates, item.TemplateName))
                {
                    return false;
                }
            }

            if (authors.Count > 0)
            {
                if (!Contains(authors, item.UpdatedBy))
                {
                    return false;
                }
            }

            if (categories.Count > 0)
            {
                if (!Contains(categories, item.Category))
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

        private void GoClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Query();
        }

        private void HandleKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key == Key.Enter)
            {
                Query();
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

            var itemVersionUri = new ItemVersionUri(selectedItem.ItemUri, LanguageManager.CurrentLanguage, Data.Version.Latest);
            AppHost.OpenContentEditor(itemVersionUri);
        }

        private void HandleMouseMove([NotNull] object sender, [NotNull] MouseEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

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
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            listViewSorter.HeaderClick(sender, e);
        }

        private void ItemDeleted([NotNull] object sender, [NotNull] ItemUri itemUri)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));

            if (hits == null)
            {
                return;
            }

            var itemSource = (ListCollectionView)Hits.ItemsSource;

            for (var index = hits.Count - 1; index >= 0; index--)
            {
                var itemHeader = hits[index];

                if (itemHeader.ItemUri == itemUri)
                {
                    hits.Remove(itemHeader);
                    itemSource.Remove(itemHeader);
                }
            }
        }

        private void ItemRenamed([NotNull] object sender, [NotNull] ItemUri itemUri, [NotNull] string newName)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(newName, nameof(newName));

            if (hits == null)
            {
                return;
            }

            var refresh = false;

            foreach (var itemHeader in hits)
            {
                if (itemHeader.ItemUri == itemUri)
                {
                    itemHeader.Name = newName;
                    refresh = true;
                }
            }

            if (refresh)
            {
                var itemSource = (ListCollectionView)Hits.ItemsSource;
                itemSource.Refresh();
            }
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

            LoadFilterValue(Templates.Items, new Tuple<string, string>(item.TemplateName, item.TemplateName));
            LoadFilterValue(Authors.Items, new Tuple<string, string>(item.UpdatedBy, item.UpdatedBy));
            LoadFilterValue(Categories.Items, new Tuple<string, string>(item.Category, item.Category));

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
            if (Templates.Items.Count == 0)
            {
                Templates.Items.Add(new ListBoxItem
                {
                    Content = Rocks.Resources.Search_LoadFilters__all_
                });
            }

            if (Authors.Items.Count == 0)
            {
                Authors.Items.Add(new ListBoxItem
                {
                    Content = Rocks.Resources.Search_LoadFilters__all_
                });
            }

            if (Categories.Items.Count == 0)
            {
                Categories.Items.Add(new ListBoxItem
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

        private void LoadHits()
        {
            Hits.Visibility = Visibility.Collapsed;
            NoHits.Visibility = Visibility.Collapsed;
            NoItems.Visibility = Visibility.Collapsed;
            NoQuery.Visibility = Visibility.Collapsed;

            var list = new List<ItemHeader>();

            ApplyFilters(list);

            Hits.ItemsSource = new ListCollectionView(list);

            if (list.Count == 0)
            {
                NoItems.Visibility = Visibility.Visible;
                return;
            }

            listViewSorter.Resort();

            Hits.Visibility = Visibility.Visible;
        }

        private void LoadHits([NotNull] IEnumerable<ItemHeader> items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            Hits.Visibility = Visibility.Collapsed;
            NoHits.Visibility = Visibility.Collapsed;
            NoItems.Visibility = Visibility.Collapsed;
            NoQuery.Visibility = Visibility.Collapsed;

            LoadFilters(items);

            hits.AddRange(items);

            if (hits.Count == 0)
            {
                NoHits.Visibility = Visibility.Visible;
                return;
            }

            LoadHits();
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = GetContext();

            ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void OpenMenu([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = GetContext();

            var contextMenu = ContextMenuExtensions.GetContextMenu(context);
            if (contextMenu == null)
            {
                return;
            }

            contextMenu.Placement = PlacementMode.Bottom;
            contextMenu.PlacementTarget = Menu;
            contextMenu.IsOpen = true;
        }

        private void Query([NotNull] ItemUri rootUri)
        {
            Debug.ArgumentNotNull(rootUri, nameof(rootUri));

            Clear();

            LastQueryText = SearchText.Text;

            if (Field.SelectedIndex < 0)
            {
                LastField = Field.Text;
            }
            else if (Field.SelectedIndex == 0)
            {
                LastField = string.Empty;
            }
            else
            {
                LastField = ((ComboBoxItem)Field.SelectedItem).Tag as string ?? string.Empty;
            }

            LastRootUri = rootUri;

            if (string.IsNullOrEmpty(LastQueryText))
            {
                return;
            }

            GuidSearchWarning.Visibility = Visibility.Collapsed;
            if (ContainsGuid(LastQueryText))
            {
                if (!AppHost.Settings.Options.HideGuidSearch)
                {
                    GuidSearchWarning.Visibility = Visibility.Visible;
                }
            }

            Query(LastQueryText, LastField, LastRootUri, PageNumber);
        }

        private void RebuildSearchIndex([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = new SearchContext(this);

            var command = new RebuildSearchIndex();

            AppHost.Usage.ReportCommand(command, context);
            command.Execute(context);
        }

        private void ResetFilters([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ResetFilters();
        }

        private void SaveGridSplitterPosition([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (FilterHeight.Height.Value > 0)
            {
                AppHost.Settings.Set("Search", "GridSplitterPosition", FilterHeight.Height.Value.ToString());
            }
        }

        private void SetFieldName([NotNull] string fieldName)
        {
            Debug.ArgumentNotNull(fieldName, nameof(fieldName));

            if (string.IsNullOrEmpty(fieldName))
            {
                Field.SelectedIndex = 0;
                return;
            }

            var f = false;

            foreach (var item in Field.Items)
            {
                var comboBoxItem = item as ComboBoxItem;
                if (comboBoxItem == null)
                {
                    continue;
                }

                if (string.Compare(comboBoxItem.Tag as string, fieldName, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    comboBoxItem.IsSelected = true;
                    f = true;
                    break;
                }
            }

            if (!f)
            {
                Field.Text = fieldName;
            }
        }

        private void ToggleFilters([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ToggleFilters();
        }

        private void ToggleFilters()
        {
            if (FilterHeight.Height.Value == 0)
            {
                var height = 100;
                var value = AppHost.Settings.Get("Search", "GridSplitterPosition", "100");
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
    }
}
