// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.UI.Packages.PluginManagers.Descriptors;

namespace Sitecore.Rocks.UI.Packages.PluginManagers.Galleries.Feeds
{
    public interface IFeed
    {
        [NotNull]
        string FeedName { get; }

        [NotNull]
        PluginManagerDialog PluginManagerDialog { get; }

        void ClearControl();

        [NotNull]
        FrameworkElement GetControl();

        void Refresh();

        void SetInstalledPlugins([NotNull] IEnumerable<BasePluginDescriptor> installedPlugins);

        void SetPage(int pageIndex);
    }
}
