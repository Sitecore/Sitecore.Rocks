// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.UI.Packages.PluginManagers.Galleries.Feeds;

namespace Sitecore.Rocks.UI.Packages.PluginManagers.Galleries
{
    [Export(typeof(IGallery), Priority = 1200)]
    public class NewGallery : GalleryBase
    {
        public NewGallery()
        {
            Name = "New";
        }

        public override IEnumerable<IFeed> GetFeeds()
        {
            var create = new CreatePlugin(PluginManagerDialog);
            yield return create;

            var upload = new MyGetUpload(PluginManagerDialog);
            yield return upload;
        }
    }
}
