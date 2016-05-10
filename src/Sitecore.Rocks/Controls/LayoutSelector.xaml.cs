// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.Controls
{
    public partial class LayoutSelector
    {
        private const string LayoutSelectorRecent = "Controls\\LayoutSelector\\Recent";

        public const string RegistryKey = "LayoutSelector";

        public LayoutSelector()
        {
            InitializeComponent();

            Loaded += ControlLoaded;
        }

        [NotNull]
        public DatabaseUri DatabaseUri { get; set; }

        [CanBeNull]
        public object SelectedItem
        {
            get { return Layouts.SelectedItem; }

            set { Layouts.SelectedItem = value; }
        }

        public void AddToRecent([NotNull] LayoutHeader layout)
        {
            Assert.ArgumentNotNull(layout, nameof(layout));

            var s = layout.Name + @"^" + layout.LayoutUri.ItemId;
            var entries = new List<string>();

            var list = AppHost.Settings.Get(LayoutSelectorRecent, GetStorageKey(), string.Empty) as string ?? string.Empty;

            if (!string.IsNullOrEmpty(list))
            {
                entries.AddRange(list.Split('|'));
            }

            entries.Remove(s);

            entries.Insert(0, s);

            while (entries.Count > 10)
            {
                entries.RemoveAt(10);
            }

            var result = string.Empty;
            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(result))
                {
                    result += @"|";
                }

                result += entry;
            }

            AppHost.Settings.Set(LayoutSelectorRecent, GetStorageKey(), result);
        }

        public event MouseButtonEventHandler DoubleClick;

        public event SelectionChangedEventHandler SelectionChanged;

        [NotNull]
        private ListBoxItem AddLayoutItem([NotNull] LayoutHeader layoutHeader)
        {
            Debug.ArgumentNotNull(layoutHeader, nameof(layoutHeader));

            var item = new ListBoxItem
            {
                Tag = layoutHeader,
                Content = layoutHeader.Name,
                ToolTip = layoutHeader.Path,
                Margin = new Thickness(16, 0, 0, 0)
            };

            Layouts.Items.Add(item);

            return item;
        }

        private void AddSection([NotNull] string name)
        {
            Debug.ArgumentNotNull(name, nameof(name));

            var sectionItem = new ListBoxItem
            {
                Content = name,
                IsEnabled = false,
                Foreground = SystemColors.HighlightBrush,
                Margin = new Thickness(0, 8, 0, 0)
            };

            Layouts.Items.Add(sectionItem);
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            DatabaseUri.Site.DataService.GetLayoutsAsync(DatabaseUri, LoadLayouts);

            Keyboard.Focus(LayoutSelectorFilter.TextBox);
            LayoutSelectorFilter.TextBox.SelectAll();
        }

        private void FilterTextChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            foreach (ListBoxItem item in Layouts.Items)
            {
                if (!item.IsEnabled)
                {
                    continue;
                }

                var layoutHeader = item.Tag as LayoutHeader;
                if (layoutHeader == null)
                {
                    continue;
                }

                item.Visibility = layoutHeader.Name.IsFilterMatch(LayoutSelectorFilter.Text) ? Visibility.Visible : Visibility.Collapsed;
            }

            var hasItems = false;

            for (var n = Layouts.Items.Count - 1; n >= 0; n--)
            {
                var item = Layouts.Items[n] as ListBoxItem;
                if (item == null)
                {
                    continue;
                }

                if (!item.IsEnabled)
                {
                    item.Visibility = hasItems ? Visibility.Visible : Visibility.Collapsed;
                    hasItems = false;
                    continue;
                }

                if (item.Visibility == Visibility.Visible)
                {
                    hasItems = true;
                }
            }
        }

        [NotNull]
        private string GetStorageKey()
        {
            return DatabaseUri.Site.Name + @"_" + DatabaseUri.DatabaseName.Name;
        }

        private void HandleMouseDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var doubleClick = DoubleClick;
            if (doubleClick != null)
            {
                doubleClick(sender, e);
            }
        }

        private void LayoutsSelectionChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var changed = SelectionChanged;
            if (changed != null)
            {
                changed(sender, e);
            }
        }

        private void LoadLayouts([NotNull] IEnumerable<LayoutHeader> layouts)
        {
            Debug.ArgumentNotNull(layouts, nameof(layouts));

            Layouts.Items.Clear();

            LoadRecent(layouts);

            string section = null;

            foreach (var layoutHeader in layouts.OrderBy(l => l.Section).ThenBy(l => l.Name))
            {
                if (layoutHeader.ParentPath != section)
                {
                    AddSection(layoutHeader.Section);
                    section = layoutHeader.Section;
                }

                AddLayoutItem(layoutHeader);
            }

            Loading.Visibility = Visibility.Collapsed;
            Layouts.Visibility = Visibility.Visible;

            var selectedItem = Layouts.Items.OfType<ListBoxItem>().FirstOrDefault();
            if (selectedItem != null)
            {
                Keyboard.Focus(selectedItem);
            }
        }

        private void LoadRecent([NotNull] IEnumerable<LayoutHeader> layouts)
        {
            Debug.ArgumentNotNull(layouts, nameof(layouts));

            var list = AppHost.Settings.Get(LayoutSelectorRecent, GetStorageKey(), string.Empty) as string;
            if (string.IsNullOrEmpty(list))
            {
                return;
            }

            var items = list.Split('|');

            AddSection(Rocks.Resources.LayoutSelector_LoadRecent_Recently_Used_Layouts);

            foreach (var item in items)
            {
                var parts = item.Split('^');

                var name = parts[0];
                var itemId = new ItemId(new Guid(parts[1]));

                var layoutUri = new ItemUri(DatabaseUri, itemId);

                var templateHeader = layouts.FirstOrDefault(header => header.LayoutUri == layoutUri);
                if (templateHeader == null)
                {
                    continue;
                }

                var layoutHeader = new LayoutHeader(layoutUri, name, templateHeader.Icon, templateHeader.Path, Rocks.Resources.Recent);

                AddLayoutItem(layoutHeader);
            }
        }
    }
}
