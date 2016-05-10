// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.Extensions;
using Sitecore.Rocks.UI.Packages.PluginManagers.Galleries.Feeds;

namespace Sitecore.Rocks.UI.Packages.PluginManagers.Galleries
{
    [Export(typeof(IGallery))]
    public class OnlineGallery : GalleryBase
    {
        public OnlineGallery()
        {
            Name = "Online";
        }

        public override IEnumerable<IFeed> GetFeeds()
        {
            foreach (var descriptor in AppHost.Plugins.GetPluginGalleries())
            {
                var source = new OnlineFeed(PluginManagerDialog, descriptor.Name, descriptor.Location);

                yield return source;
            }
        }
    }
}
