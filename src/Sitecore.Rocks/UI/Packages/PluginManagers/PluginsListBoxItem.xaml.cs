// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.Net.Cache;
using System.Windows;
using System.Windows.Media.Imaging;
using NuGet;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Packages.PluginManagers.Descriptors;
using Sitecore.Rocks.UI.Packages.PluginManagers.Galleries.Feeds;

namespace Sitecore.Rocks.UI.Packages.PluginManagers
{
    public partial class PluginsListBoxItem
    {
        public PluginsListBoxItem([NotNull] IFeed feed, [NotNull] BasePluginDescriptor plugin, bool canInstall, bool canUninstall, bool canUpdate, bool isInstalled)
        {
            Assert.ArgumentNotNull(feed, nameof(feed));
            Assert.ArgumentNotNull(plugin, nameof(plugin));

            InitializeComponent();

            DataContext = plugin;

            Feed = feed;
            Plugin = plugin;
            CanInstall = canInstall;
            CanUninstall = canUninstall;
            CanUpdate = canUpdate;
            IsInstalled = isInstalled;

            Plugin.PropertyChanged += HandlePropertyChanged;

            var packagePlugin = plugin as PackagePluginDescriptor;
            if (packagePlugin != null)
            {
                PrereleaseTextBlock.Visibility = packagePlugin.Package.IsReleaseVersion() ? Visibility.Collapsed : Visibility.Visible;
            }

            Refresh();
        }

        public bool CanInstall { get; private set; }

        public bool CanUninstall { get; }

        public bool CanUpdate { get; private set; }

        [NotNull]
        public IFeed Feed { get; }

        public bool IsInstalled { get; private set; }

        [NotNull]
        public BasePluginDescriptor Plugin { get; }

        public void Refresh()
        {
            NameTextBlock.Text = Plugin.Title;
            SummaryTextBlock.Text = Plugin.Summary;

            InstallButton.Visibility = CanInstall && !IsInstalled ? Visibility.Visible : Visibility.Collapsed;
            UninstallButton.Visibility = CanUninstall ? Visibility.Visible : Visibility.Collapsed;
            UpdateButton.Visibility = CanUpdate ? Visibility.Visible : Visibility.Collapsed;

            if (IsInstalled)
            {
                InstalledImage.Visibility = Visibility.Visible;
                InstalledImage.Source = new Icon("Sitecore.Rocks.Plugins", "/Resources/16x16/db-post.png").GetSource();
            }
            else
            {
                InstalledImage.Visibility = Visibility.Collapsed;
            }

            var url = Plugin.IconUrl;
            if (url == null)
            {
                LoadDefaultIcon();
                return;
            }

            var policy = new RequestCachePolicy(RequestCacheLevel.Default);
            try
            {
                var bitmapImage = new BitmapImage(url, policy);
                IconImage.ImageFailed += LoadDefaultIcon;
                IconImage.Source = bitmapImage;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }

        public void SetSelected([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            ButtonPanel.Visibility = Visibility.Visible;
        }

        public void SetUnselected([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            ButtonPanel.Visibility = Visibility.Collapsed;
        }

        private void HandlePropertyChanged([NotNull] object sender, [NotNull] PropertyChangedEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));
            Debug.ArgumentNotNull(sender, nameof(sender));

            if (e.PropertyName == "IsInstalled")
            {
                Refresh();
            }
        }

        private void Install([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));
            Debug.ArgumentNotNull(sender, nameof(sender));

            Action completed = delegate
            {
                IsInstalled = true;
                CanInstall = false;
                RefreshList();
            };

            Plugin.Install(completed);
        }

        private void LoadDefaultIcon([NotNull] object sender, [NotNull] ExceptionRoutedEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));
            Debug.ArgumentNotNull(sender, nameof(sender));

            LoadDefaultIcon();
        }

        private void LoadDefaultIcon()
        {
            IconImage.ImageFailed -= LoadDefaultIcon;

            var icon = new Icon("Sitecore.Rocks.Plugins", "/Resources/48x48/socket_48.png");

            IconImage.Source = icon.GetSource();
        }

        private void RefreshList()
        {
            Feed.PluginManagerDialog.Refresh();
            Refresh();
        }

        private void Uninstall([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));
            Debug.ArgumentNotNull(sender, nameof(sender));

            Action completed = delegate
            {
                IsInstalled = false;
                RefreshList();
            };

            Plugin.Uninstall(completed);
        }

        private void Update([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));
            Debug.ArgumentNotNull(sender, nameof(sender));

            Action completed = delegate
            {
                CanUpdate = false;
                RefreshList();
            };

            Plugin.Update(completed);
        }
    }
}
