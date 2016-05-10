// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.UI.Packages.PluginManagers.Galleries.Feeds;

namespace Sitecore.Rocks.UI.Packages.PluginManagers.Galleries
{
    [Export(typeof(IGallery))]
    public class UpdatesGallery : GalleryBase
    {
        public UpdatesGallery()
        {
            Name = "Updates";
        }

        public override IEnumerable<IFeed> GetFeeds()
        {
            var source = new UpdatesFeed(PluginManagerDialog, "All", AppHost.Plugins.PackageFolder);

            yield return source;
        }
    }
}
