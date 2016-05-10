// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NuGet;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.UI.Packages.PluginManagers.Descriptors;
using Sitecore.Rocks.UI.Packages.PluginManagers.Dialogs.PluginFolders;
using Sitecore.Rocks.UI.Packages.PluginManagers.Dialogs.PluginGalleries;
using Sitecore.Rocks.UI.Packages.PluginManagers.Galleries;
using Sitecore.Rocks.UI.Packages.PluginManagers.Galleries.Feeds;

namespace Sitecore.Rocks.UI.Packages.PluginManagers
{
    public partial class PluginManagerDialog
    {
        [NotNull]
        private readonly List<IFeed> feeds = new List<IFeed>();

        [NotNull]
        private readonly List<BasePluginDescriptor> installedPlugins = new List<BasePluginDescriptor>();

        public PluginManagerDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            AppHost.Extensibility.ComposeParts(this);

            Loaded += ControlLoaded;
        }

        [CanBeNull]
        public IFeed CurrentFeed { get; set; }

        [NotNull, ImportMany(typeof(IGallery))]
        public IEnumerable<IGallery> Galleries { get; set; }

        [NotNull]
        public IEnumerable<BasePluginDescriptor> InstalledPlugins
        {
            get { return installedPlugins; }
        }

        public void Refresh()
        {
            RefreshInstalledPlugins();

            foreach (var feed in feeds)
            {
                feed.SetInstalledPlugins(InstalledPlugins);
            }

            if (CurrentFeed != null)
            {
                CurrentFeed.Refresh();
            }
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            RefreshInstalledPlugins();

            RenderGalleries();
        }

        private void EditFeeds([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));
            Debug.ArgumentNotNull(sender, nameof(sender));

            var d = new PluginGalleryDialog();
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            RenderGalleries();
        }

        private void EditFolders([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));
            Debug.ArgumentNotNull(sender, nameof(sender));

            var d = new PluginFoldersDialog();
            AppHost.Shell.ShowDialog(d);

            RefreshInstalledPlugins();
            AppHost.Extensibility.Reinitialize();
            RenderGalleries();
        }

        [CanBeNull]
        private AssemblyPluginDescriptor GetAssemblyPluginDescriptor([NotNull] string fileName)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));

            var assembly = AppHost.Plugins.SafeLoadAssembly(fileName);
            if (assembly == null)
            {
                return null;
            }

            return new AssemblyPluginDescriptor(assembly);
        }

        private void RefreshInstalledPlugins()
        {
            var pluginDescriptors = new List<BasePluginDescriptor>();

            var fileNames = new List<string>();
            AppHost.Plugins.GetAssemblies(fileNames, false, true, false);
            pluginDescriptors.AddRange(fileNames.Select(GetAssemblyPluginDescriptor).Where(p => p != null));

            var repository = new SharedPackageRepository(AppHost.Plugins.PackageFolder);
            pluginDescriptors.AddRange(repository.GetPackages().Select(p => new PackagePluginDescriptor(repository, p)).OfType<BasePluginDescriptor>());

            pluginDescriptors.Sort((x, y) => string.Compare(x.Title, y.Title, StringComparison.InvariantCultureIgnoreCase));

            installedPlugins.Clear();
            installedPlugins.AddRange(pluginDescriptors);
        }

        private void RenderGalleries()
        {
            Feeds.Items.Clear();

            TreeViewItem selectedItem = null;

            var galleries = Galleries.ToList();

            foreach (var gallery in galleries)
            {
                gallery.Initialize(this);

                var treeViewItem = new TreeViewItem
                {
                    Header = gallery.Name,
                    IsExpanded = true
                };

                if (Feeds.Items.Count > 0)
                {
                    treeViewItem.Margin = new Thickness(0, 8, 0, 0);
                }

                Feeds.Items.Add(treeViewItem);

                foreach (var feed in gallery.GetFeeds())
                {
                    feeds.Add(feed);
                    feed.SetInstalledPlugins(InstalledPlugins);

                    var item = new TreeViewItem
                    {
                        Header = feed.FeedName,
                        Tag = feed,
                        FontWeight = FontWeights.Normal
                    };

                    treeViewItem.Items.Add(item);

                    if (selectedItem == null)
                    {
                        selectedItem = item;
                    }
                }
            }

            if (selectedItem != null)
            {
                selectedItem.IsSelected = true;
            }
        }

        private void SetFeed([NotNull] object sender, [NotNull] RoutedPropertyChangedEventArgs<object> e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var treeViewItem = Feeds.SelectedItem as TreeViewItem;
            if (treeViewItem == null)
            {
                return;
            }

            var feed = treeViewItem.Tag as IFeed;
            if (feed == null)
            {
                var firstChild = treeViewItem.Items.OfType<TreeViewItem>().FirstOrDefault();
                if (firstChild == null)
                {
                    return;
                }

                feed = firstChild.Tag as IFeed;
                if (feed == null)
                {
                    return;
                }

                firstChild.IsSelected = true;
                return;
            }

            CurrentFeed = feed;
            var control = feed.GetControl();
            Feed.Child = control;
            feed.Refresh();
        }
    }
}
