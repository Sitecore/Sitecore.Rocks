// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.Packages.PackageManagers.Sources;

namespace Sitecore.Rocks.UI.Packages.PackageManagers
{
    public partial class PackageListBox
    {
        public PackageListBox()
        {
            InitializeComponent();

            PackageInformationPanel.Clear();
            Filter.Text = string.Empty;
        }

        public void Clear()
        {
            PackageInformationPanel.Clear();
            PackageList.Items.Clear();
        }

        public void RenderPackages([NotNull] IEnumerable<PackageInformation> packages, [NotNull] Action<PackageInformation> install)
        {
            Assert.ArgumentNotNull(packages, nameof(packages));
            Assert.ArgumentNotNull(install, nameof(install));

            PackageList.Items.Clear();

            var isSelected = true;

            foreach (var package in packages)
            {
                var packageListBoxItem = new PackageListBoxItem(package, install);

                var listBoxItem = new ListBoxItem
                {
                    VerticalContentAlignment = VerticalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Content = packageListBoxItem,
                    Tag = package,
                    Visibility = package.PackageName.IsFilterMatch(Filter.Text) ? Visibility.Visible : Visibility.Collapsed
                };

                listBoxItem.Selected += packageListBoxItem.SetSelected;
                listBoxItem.Unselected += packageListBoxItem.SetUnselected;

                listBoxItem.IsSelected = isSelected;

                PackageList.Items.Add(listBoxItem);

                isSelected = false;
            }

            Loading.HideLoading(PackageList, NoPackages, InfoPanel);

            NoPackages.Visibility = PackageList.Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            PackageList.Visibility = PackageList.Items.Count != 0 ? Visibility.Visible : Visibility.Collapsed;
            InfoPanel.Visibility = PackageList.Items.Count != 0 ? Visibility.Visible : Visibility.Collapsed;
            GridSplitter.Visibility = PackageList.Items.Count != 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ShowActionButton([NotNull] string content, [NotNull] RoutedEventHandler action)
        {
            Assert.ArgumentNotNull(content, nameof(content));
            Assert.ArgumentNotNull(action, nameof(action));

            ActionButton.Content = content;
            ActionButton.Visibility = Visibility.Visible;
            ActionButton.Click += action;
        }

        private void FilterPackages(object sender, EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            foreach (ListBoxItem item in PackageList.Items)
            {
                if (!item.IsEnabled)
                {
                    continue;
                }

                var package = item.Tag as PackageInformation;
                if (package == null)
                {
                    continue;
                }

                item.Visibility = package.PackageName.IsFilterMatch(Filter.Text) ? Visibility.Visible : Visibility.Collapsed;
            }

            var hasItems = false;

            for (var n = PackageList.Items.Count - 1; n >= 0; n--)
            {
                var item = PackageList.Items[n] as ListBoxItem;
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

        private void SetPackage([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            PackageInformationPanel.Clear();

            var selecteditem = PackageList.SelectedItem as ListBoxItem;
            if (selecteditem == null)
            {
                return;
            }

            var packageListBoxItem = selecteditem.Content as PackageListBoxItem;
            if (packageListBoxItem == null)
            {
                return;
            }

            PackageInformationPanel.Package = packageListBoxItem.Package;
        }
    }
}
