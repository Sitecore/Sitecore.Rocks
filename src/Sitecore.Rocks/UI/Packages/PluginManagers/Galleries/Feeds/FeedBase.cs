// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Packages.PluginManagers.Descriptors;

namespace Sitecore.Rocks.UI.Packages.PluginManagers.Galleries.Feeds
{
    public abstract class FeedBase : IFeed
    {
        protected FeedBase([NotNull] PluginManagerDialog pluginManagerDialog)
        {
            Debug.ArgumentNotNull(pluginManagerDialog, nameof(pluginManagerDialog));

            PluginManagerDialog = pluginManagerDialog;
        }

        public string FeedName { get; protected set; }

        public PluginManagerDialog PluginManagerDialog { get; }

        [NotNull]
        protected IEnumerable<BasePluginDescriptor> InstalledPlugins { get; private set; }

        public abstract void ClearControl();

        public abstract FrameworkElement GetControl();

        public abstract void Refresh();

        public virtual void SetInstalledPlugins(IEnumerable<BasePluginDescriptor> installedPlugins)
        {
            Assert.ArgumentNotNull(installedPlugins, nameof(installedPlugins));

            InstalledPlugins = installedPlugins;
        }

        public virtual void SetPage(int pageIndex)
        {
        }
    }
}
