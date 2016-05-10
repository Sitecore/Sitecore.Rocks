// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Packages.PluginManagers.Galleries.Feeds;

namespace Sitecore.Rocks.UI.Packages.PluginManagers.Galleries
{
    public abstract class GalleryBase : IGallery
    {
        public string Name { get; protected set; }

        [NotNull]
        protected PluginManagerDialog PluginManagerDialog { get; private set; }

        public abstract IEnumerable<IFeed> GetFeeds();

        public virtual void Initialize(PluginManagerDialog pluginManagerDialog)
        {
            Assert.ArgumentNotNull(pluginManagerDialog, nameof(pluginManagerDialog));

            PluginManagerDialog = pluginManagerDialog;
        }
    }
}
