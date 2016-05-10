// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Packages.PluginManagers.Galleries.Feeds;

namespace Sitecore.Rocks.UI.Packages.PluginManagers
{
    public partial class PluginsListBox
    {
        private bool supportPrereleases;

        public PluginsListBox([NotNull] IFeed feed)
        {
            Assert.ArgumentNotNull(feed, nameof(feed));

            InitializeComponent();

            Feed = feed;
            PackageInformationPanel.Clear();
            Search.Text = string.Empty;
        }

        [NotNull]
        public IFeed Feed { get; }

        public bool IncludePrereleases
        {
            get { return PrereleasesComboBox.SelectedIndex == 1; }
        }

        [NotNull]
        public string SearchText
        {
            get { return Search.Text; }
        }

        public bool SupportPrereleases
        {
            get { return supportPrereleases; }

            set
            {
                supportPrereleases = value;
                PrereleasesComboBox.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public void Clear()
        {
            PackageInformationPanel.Clear();
            PluginList.Items.Clear();
        }

        public void RenderPager(int currentPage, int firstPage, int lastPage)
        {
            PagerStackPanel.Visibility = Visibility.Visible;
            PagerStackPanel.Children.Clear();

            if (currentPage > 0)
            {
                var button = new Button
                {
                    Content = "<",
                    Width = 24,
                    Height = 21,
                    Margin = new Thickness(2, 0, 2, 0),
                    Tag = currentPage - 1
                };

                button.Click += SetPage;

                PagerStackPanel.Children.Add(button);
            }

            for (var n = firstPage; n <= lastPage; n++)
            {
                var button = new Button
                {
                    Content = (n + 1).ToString(),
                    Width = 24,
                    Height = 21,
                    Margin = new Thickness(2, 0, 2, 0),
                    Tag = n
                };

                button.Click += SetPage;

                PagerStackPanel.Children.Add(button);
            }

            if (currentPage < lastPage)
            {
                var button = new Button
                {
                    Content = ">",
                    Width = 24,
                    Height = 21,
                    Margin = new Thickness(2, 0, 2, 0),
                    Tag = currentPage + 1
                };

                button.Click += SetPage;

                PagerStackPanel.Children.Add(button);
            }
        }

        public void RenderPlugins([NotNull] IEnumerable<PluginsListBoxItem> listBoxItems)
        {
            Assert.ArgumentNotNull(listBoxItems, nameof(listBoxItems));

            PluginList.Items.Clear();

            var isSelected = true;

            foreach (var item in listBoxItems)
            {
                var listBoxItem = new ListBoxItem
                {
                    VerticalContentAlignment = VerticalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Content = item,
                    Tag = item.Plugin,
                };

                listBoxItem.Selected += item.SetSelected;
                listBoxItem.Unselected += item.SetUnselected;

                listBoxItem.IsSelected = isSelected;

                PluginList.Items.Add(listBoxItem);

                isSelected = false;
            }

            Loading.HideLoading(PackageListPane, NoPackages, InfoPanel);

            NoPackages.Visibility = PluginList.Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            PluginList.Visibility = PluginList.Items.Count != 0 ? Visibility.Visible : Visibility.Collapsed;
            InfoPanel.Visibility = PluginList.Items.Count != 0 ? Visibility.Visible : Visibility.Collapsed;
            GridSplitter.Visibility = PluginList.Items.Count != 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void FilterPackages([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));
            Debug.ArgumentNotNull(sender, nameof(sender));

            Feed.Refresh();
        }

        private void SetPackage([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            PackageInformationPanel.Clear();

            var selecteditem = PluginList.SelectedItem as ListBoxItem;
            if (selecteditem == null)
            {
                return;
            }

            var packageListBoxItem = selecteditem.Content as PluginsListBoxItem;
            if (packageListBoxItem == null)
            {
                return;
            }

            PackageInformationPanel.Plugin = packageListBoxItem.Plugin;
        }

        private void SetPage([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));
            Debug.ArgumentNotNull(sender, nameof(sender));

            var button = sender as Button;
            if (button == null)
            {
                return;
            }

            var page = (int)button.Tag;

            Feed.SetPage(page);
        }

        private void SetPrerelease([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));
            Debug.ArgumentNotNull(sender, nameof(sender));

            if (Search != null)
            {
                Feed.Refresh();
            }
        }
    }
}
