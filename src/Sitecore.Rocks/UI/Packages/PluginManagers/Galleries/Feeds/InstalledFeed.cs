// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.UI.Packages.PluginManagers.Galleries.Feeds
{
    public class InstalledFeed : NuGetFeed
    {
        public InstalledFeed([NotNull] PluginManagerDialog pluginManagerDialog, [NotNull] string feedName, [NotNull] string url) : base(pluginManagerDialog, feedName, url)
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
            var listBoxItems = new List<PluginsListBoxItem>();

            foreach (var plugin in InstalledPlugins)
            {
                if (!plugin.Title.IsFilterMatch(searchText))
                {
                    continue;
                }

                listBoxItems.Add(new PluginsListBoxItem(this, plugin, false, true, false, false));
            }

            PluginsListBox.RenderPlugins(listBoxItems);
        }
    }
}
