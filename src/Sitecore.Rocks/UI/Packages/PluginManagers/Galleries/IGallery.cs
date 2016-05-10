// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.UI.Packages.PluginManagers.Galleries.Feeds;

namespace Sitecore.Rocks.UI.Packages.PluginManagers.Galleries
{
    public interface IGallery
    {
        [NotNull]
        string Name { get; }

        [NotNull]
        IEnumerable<IFeed> GetFeeds();

        void Initialize([NotNull] PluginManagerDialog pluginManagerDialog);
    }
}
