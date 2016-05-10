// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Packages.PluginManagers.Descriptors;

namespace Sitecore.Rocks.UI.Packages.PluginManagers.Galleries.Feeds
{
    public partial class MyGetUpload : IFeed
    {
        public MyGetUpload([NotNull] PluginManagerDialog pluginDialogDialog)
        {
            Assert.ArgumentNotNull(pluginDialogDialog, nameof(pluginDialogDialog));
            InitializeComponent();

            FeedName = "Upload to MyGet.org";
            PluginManagerDialog = pluginDialogDialog;
        }

        [NotNull]
        public string FeedName { get; }

        public PluginManagerDialog PluginManagerDialog { get; }

        public void ClearControl()
        {
        }

        public FrameworkElement GetControl()
        {
            return this;
        }

        public void Refresh()
        {
        }

        public void SetInstalledPlugins(IEnumerable<BasePluginDescriptor> installedPlugins)
        {
            Assert.ArgumentNotNull(installedPlugins, nameof(installedPlugins));
        }

        public void SetPage(int pageIndex)
        {
        }

        private void GotoMyGet([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Browsers.Navigate("https://www.myget.org/gallery/sitecore-rocks-v2");
        }
    }
}
