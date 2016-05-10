// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ContextMenuExtensions;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Media.Commands;
using Sitecore.Rocks.Media.Skins;
using Sitecore.Rocks.Searching;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Media
{
    public partial class MediaViewer : IContextProvider
    {
        private readonly List<ItemHeader> hits = new List<ItemHeader>();

        private TreeViewItem pathsAllTreeViewItem;

        private string skinName;

        private TreeViewItem updatedAllTreeViewItem;

        public MediaViewer()
        {
            InitializeComponent();

            SkinName = AppHost.Settings.Get("Media", "Skin", string.Empty) as string ?? string.Empty;

            TemplateLabel.MouseDown += (sender, args) => Templates.SelectedIndex = 0;
            AuthorLabel.MouseDown += (sender, args) => Authors.SelectedIndex = 0;
            PathLabel.MouseDown += (sender, args) =>
            {
                if (pathsAllTreeViewItem != null)
                {
                    pathsAllTreeViewItem.IsSelected = true;
                }
            };

            UpdatedLabel.MouseDown += (sender, args) => updatedAllTreeViewItem.IsSelected = true;

            Loaded += ControlLoaded;

            Notifications.RegisterItemEvents(this, deleted: ItemDeleted, renamed: ItemRenamed);
        }

        [NotNull]
        public DatabaseUri DatabaseUri
        {
            get { return new DatabaseUri(Site, DatabaseName.Master); }
        }

        public string LastField { get; private set; }

        public string LastQueryText { get; private set; }

        public int PageNumber { get; private set; }

        public IPane Pane { get; set; }

        public Site Site { get; private set; }

        [NotNull]
        public string SkinName
        {
            get { return skinName ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                if (skinName == value)
                {
                    return;
                }

                skinName = value;
                AppHost.Settings.Set("Media", "Skin", value);

                SkinPanel.Children.Clear();

                Skin = MediaSkinManager.GetInstance(skinName) ?? MediaSkinManager.GetDefaultInstance();

                Skin.Initialize(this);
                Skin.Site = Site;

                SkinPanel.Children.Add(Skin.GetControl());

                ApplyFilters();
            }
        }

        private IMediaSkin Skin { get; set; }

        public void AddItem([NotNull] ItemHeader itemHeader)
        {
            Assert.ArgumentNotNull(itemHeader, nameof(itemHeader));

            hits.Add(itemHeader);
        }

        [NotNull]
        public object GetContext()
        {
            return new MediaContext(this)
            {
                SelectedItems = Skin.GetSelectedItems()
            };
        }

        public void Initialize([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            SetSite(site);
            Skin.Site = site;
        }

        public void NextPage()
        {
            PageNumber++;

            Search(LastQueryText, LastField, PageNumber);
        }

        public void ResetFilters()
        {
            Templates.SelectedIndex = 0;
            Authors.SelectedIndex = 0;

            if (pathsAllTreeViewItem != null)
            {
                pathsAllTreeViewItem.IsSelected = true;
            }

            if (updatedAllTreeViewItem != null)
            {
                updatedAllTreeViewItem.IsSelected = true;
            }
        }

        public void Search([NotNull] string queryText, [NotNull] string field, int pageNumber)
        {
            Assert.ArgumentNotNull(queryText, nameof(queryText));
            Assert.ArgumentNotNull(field, nameof(field));

            if (string.IsNullOrEmpty(queryText))
            {
                queryText = @"_updated:[00010101 TO 21003112]";
            }

            queryText = queryText.Replace(@"\", @"\\");

            var root = new ItemUri(new DatabaseUri(Site, DatabaseName.Master), IdManager.GetItemId("/sitecore/media library"));

            Site.DataService.SearchMedia(queryText, root.DatabaseUri, field, string.Empty, root, pageNumber, LoadHits);
        }

        public void Search([Localizable(false), NotNull] string fieldName, [NotNull] string value)
        {
            Assert.ArgumentNotNull(fieldName, nameof(fieldName));
            Assert.ArgumentNotNull(value, nameof(value));

            SearchText.Text = value;
            SetFieldName(fieldName);

            Search();
        }

        public void Search()
        {
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

            GuidSearchWarning.Visibility = Visibility.Collapsed;
            if (ContainsGuid(LastQueryText))
            {
                if (!AppHost.Settings.Options.HideGuidSearch)
                {
                    GuidSearchWarning.Visibility = Visibility.Visible;
                }
            }

            Search(LastQueryText, LastField, PageNumber);
        }

        public void SetSite([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            Site = site;
            Pane.Caption = @"Media Library [" + site.Name + "]";
            Clear();
        }

        private void ApplyFilters()
        {
            SkinPanel.Visibility = Visibility.Collapsed;
            NoHits.Visibility = Visibility.Collapsed;
            NoItems.Visibility = Visibility.Collapsed;
            NoQuery.Visibility = Visibility.Collapsed;

            var templates = new List<string>();
            var authors = new List<string>();
            var path = string.Empty;
            var year = 0;
            var month = 0;
            var day = 0;

            GetFilter(Templates.Items, templates);
            GetFilter(Authors.Items, authors);

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

            var list = new List<ItemHeader>();

            ApplyFilters(list, templates, authors, path, year, month, day);

            Skin.Clear();
            Skin.Load(list);

            if (list.Count == 0)
            {
                NoItems.Visibility = Visibility.Visible;
                return;
            }

            SkinPanel.Visibility = Visibility.Visible;
        }

        private void ApplyFilters([NotNull] List<ItemHeader> list, [NotNull] List<string> templates, [NotNull] List<string> authors, [NotNull] string path, int year, int month, int day)
        {
            Debug.ArgumentNotNull(list, nameof(list));
            Debug.ArgumentNotNull(templates, nameof(templates));
            Debug.ArgumentNotNull(authors, nameof(authors));
            Debug.ArgumentNotNull(path, nameof(path));

            foreach (var item in hits)
            {
                if (GetVisible(item, templates, authors, path, year, month, day))
                {
                    list.Add(item);
                }
            }
        }

        private void ApplyFilters([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ApplyFilters();
        }

        private void ApplyFilters([NotNull] object sender, [NotNull] RoutedPropertyChangedEventArgs<object> e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ApplyFilters();
        }

        private void Clear()
        {
            hits.Clear();
            PageNumber = 0;

            Skin.Clear();
            Templates.Items.Clear();
            Authors.Items.Clear();
            Paths.Items.Clear();
            Updated.Items.Clear();
        }

        private bool Contains([NotNull] List<string> items, [NotNull] string value)
        {
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(value, nameof(value));

            foreach (var template in items)
            {
                if (string.Compare(template, value, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        private bool ContainsGuid([NotNull] string expression)
        {
            Debug.ArgumentNotNull(expression, nameof(expression));

            var guidRegEx = new Regex(@"(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})");

            return guidRegEx.IsMatch(expression);
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            if (Site != null)
            {
                Search();
            }
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

        private bool GetVisible([NotNull] ItemHeader item, [NotNull] List<string> templates, [NotNull] List<string> authors, [NotNull] string path, int year, int month, int day)
        {
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(templates, nameof(templates));
            Debug.ArgumentNotNull(authors, nameof(authors));
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

            Search();
        }

        private void HandleKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key == Key.Enter)
            {
                Search();
            }
        }

        private void ItemDeleted([NotNull] object sender, [NotNull] ItemUri itemUri)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));

            if (hits == null)
            {
                return;
            }

            for (var index = hits.Count - 1; index >= 0; index--)
            {
                var itemHeader = hits[index];
                if (itemHeader.ItemUri != itemUri)
                {
                    continue;
                }

                hits.Remove(itemHeader);
                Skin.Deleted(itemHeader);
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

            foreach (var itemHeader in hits.Where(itemHeader => itemHeader.ItemUri == itemUri))
            {
                itemHeader.Name = newName;
                Skin.Renamed(itemHeader, newName);
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

            var path = string.Empty;
            var itemPath = item.Path;
            if (itemPath.StartsWith(@"/sitecore/media library", StringComparison.InvariantCultureIgnoreCase))
            {
                itemPath = itemPath.Mid(24);
                path = @"/sitecore/media library";
            }

            var parts = new List<Tuple<string, string>>();
            foreach (var s in itemPath.Split('/'))
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

            var y = new Tuple<string, string>(year, year);
            var m = new Tuple<string, string>(year + @"/" + month, item.Updated.ToString(@"MMMM"));
            var d = new Tuple<string, string>(year + @"/" + month + @"/" + day, day);

            var tuples = new List<Tuple<string, string>>
            {
                y,
                m,
                d
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

        private void LoadHits([NotNull] IEnumerable<ItemHeader> items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            SkinPanel.Visibility = Visibility.Collapsed;
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

            ApplyFilters();
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = GetContext();

            ContextMenuHolder.ContextMenu = AppHost.ContextMenus.Build(context, e);
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

        private void RebuildSearchIndex([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = new MediaContext(this);

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
