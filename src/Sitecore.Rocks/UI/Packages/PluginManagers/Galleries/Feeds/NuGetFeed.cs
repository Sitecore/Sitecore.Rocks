// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Packages.PluginManagers.Galleries.Feeds
{
    public abstract class NuGetFeed : FeedBase
    {
        private PluginsListBox pluginsListBox;

        protected NuGetFeed([NotNull] PluginManagerDialog pluginManagerDialog, [NotNull] string feedName, [NotNull] string url) : base(pluginManagerDialog)
        {
            Debug.ArgumentNotNull(pluginManagerDialog, nameof(pluginManagerDialog));
            Debug.ArgumentNotNull(feedName, nameof(feedName));
            Debug.ArgumentNotNull(url, nameof(url));

            FeedName = feedName;
            Url = url;

            PageIndex = 0;
        }

        public int PageIndex { get; set; }

        [NotNull]
        public PluginsListBox PluginsListBox
        {
            get { return pluginsListBox ?? (pluginsListBox = new PluginsListBox(this)); }
        }

        [NotNull]
        public string Url { get; private set; }

        public override void ClearControl()
        {
            pluginsListBox = null;
        }

        public override FrameworkElement GetControl()
        {
            return PluginsListBox;
        }

        public override void Refresh()
        {
            PageIndex = 0;

            RenderPackages();
        }

        public override void SetPage(int pageIndex)
        {
            PageIndex = pageIndex;

            RenderPackages();
        }

        protected abstract void RenderPackages();
    }
}
