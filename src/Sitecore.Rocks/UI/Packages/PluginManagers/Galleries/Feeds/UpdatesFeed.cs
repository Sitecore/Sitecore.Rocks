// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using NuGet;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.Packages.PluginManagers.Descriptors;

namespace Sitecore.Rocks.UI.Packages.PluginManagers.Galleries.Feeds
{
    public class UpdatesFeed : NuGetFeed
    {
        public UpdatesFeed([NotNull] PluginManagerDialog pluginManagerDialog, [NotNull] string feedName, [NotNull] string url) : base(pluginManagerDialog, feedName, url)
        {
            Assert.ArgumentNotNull(pluginManagerDialog, nameof(pluginManagerDialog));
            Assert.ArgumentNotNull(feedName, nameof(feedName));
            Assert.ArgumentNotNull(url, nameof(url));
        }

        protected override void RenderPackages()
        {
            PluginsListBox.Clear();

            PluginsListBox.Loading.ShowLoading(PluginsListBox.PackageListPane);
            AppHost.DoEvents();

            var searchText = PluginsListBox.Search.Text;
            var packages = InstalledPlugins.OfType<PackagePluginDescriptor>().Select(p => p.Package).ToList();

            var updatePackages = new List<Tuple<IPackageRepository, IPackage>>();

            foreach (var gallery in AppHost.Plugins.GetPluginGalleries())
            {
                var repository = PackageRepositoryFactory.Default.CreateRepository(gallery.Location);

                List<IPackage> updates;
                try
                {
                    updates = repository.GetUpdates(packages, PluginsListBox.IncludePrereleases, false).ToList();
                }
                catch (ArgumentException ex)
                {
                    AppHost.Output.LogException(ex);
                    continue;
                }

                updatePackages.AddRange(updates.Select(package => new Tuple<IPackageRepository, IPackage>(repository, package)));
            }

            var listBoxItems = new List<PluginsListBoxItem>();

            foreach (var tuple in updatePackages)
            {
                if (!tuple.Item2.Title.IsFilterMatch(searchText))
                {
                    continue;
                }

                var plugin = new PackagePluginDescriptor(tuple.Item1, tuple.Item2);
                listBoxItems.Add(new PluginsListBoxItem(this, plugin, false, false, true, false));
            }

            PluginsListBox.RenderPlugins(listBoxItems);
        }
    }
}
