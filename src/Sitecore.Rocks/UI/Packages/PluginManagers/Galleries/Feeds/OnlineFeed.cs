// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using NuGet;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Packages.PluginManagers.Descriptors;

namespace Sitecore.Rocks.UI.Packages.PluginManagers.Galleries.Feeds
{
    public class OnlineFeed : NuGetFeed
    {
        public OnlineFeed([NotNull] PluginManagerDialog pluginManagerDialog, [NotNull] string feedName, [NotNull] string url) : base(pluginManagerDialog, feedName, url)
        {
            Assert.ArgumentNotNull(pluginManagerDialog, nameof(pluginManagerDialog));
            Assert.ArgumentNotNull(feedName, nameof(feedName));
            Assert.ArgumentNotNull(url, nameof(url));
        }

        protected override void RenderPackages()
        {
            PluginsListBox.Clear();
            PluginsListBox.SupportPrereleases = true;

            PluginsListBox.Loading.ShowLoading(PluginsListBox.PackageListPane);
            AppHost.DoEvents();

            var plugins = new List<BasePluginDescriptor>();

            IPackageRepository repository;
            try
            {
                repository = PackageRepositoryFactory.Default.CreateRepository(Url);
            }
            catch (Exception ex)
            {
                AppHost.Output.LogException(ex);
                AppHost.MessageBox(string.Format("The URL of the repository \"{0}\" is invalid:\n\n{1}\n\n{2}", FeedName, Url, ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var packages = new List<IPackage>();
            try
            {
                var query = repository.Search(PluginsListBox.Search.Text, PluginsListBox.IncludePrereleases);

                query = query.OrderByDescending(p => p.DownloadCount).ThenBy(p => p.Title).Skip(PageIndex * 10);

                packages = query.ToList().GroupBy(p => p.Id).Select(y => y.OrderByDescending(p => p.Version).First()).ToList();
            }
            catch (WebException ex)
            {
                AppHost.MessageBox("Failed to communicated with the server: " + Url + "\n\nThe server responded with: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (UriFormatException)
            {
                AppHost.MessageBox("The URL is invalid: " + Url, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            var listBoxItems = new List<PluginsListBoxItem>();

            var count = 0;
            foreach (var package in packages)
            {
                var plugin = new PackagePluginDescriptor(repository, package);
                plugins.Add(plugin);

                count++;
                if (count == 10)
                {
                    break;
                }

                var isInstalled = InstalledPlugins.OfType<PackagePluginDescriptor>().Any(p => p.Package.Id == package.Id && p.Package.Version == package.Version);

                listBoxItems.Add(new PluginsListBoxItem(this, plugin, true, false, false, isInstalled));
            }

            var total = PageIndex * 10 + packages.Count;

            var currentPage = PageIndex;
            var firstPage = currentPage - 2;
            if (firstPage < 0)
            {
                firstPage = 0;
            }

            var lastPage = total % 10 == 0 ? total / 10 - 1 : total / 10;
            if (lastPage > firstPage + 4)
            {
                lastPage = firstPage + 4;
            }

            PluginsListBox.RenderPlugins(listBoxItems);

            PluginsListBox.RenderPager(currentPage, firstPage, lastPage);
        }
    }
}
