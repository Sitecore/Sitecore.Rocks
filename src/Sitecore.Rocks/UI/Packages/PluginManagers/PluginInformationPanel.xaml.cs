// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Net.Cache;
using System.Windows;
using System.Windows.Media.Imaging;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Packages.PluginManagers.Descriptors;

namespace Sitecore.Rocks.UI.Packages.PluginManagers
{
    public partial class PluginInformationPanel
    {
        private BasePluginDescriptor plugin;

        public PluginInformationPanel()
        {
            InitializeComponent();
        }

        [CanBeNull]
        public BasePluginDescriptor Plugin
        {
            get { return plugin; }

            set
            {
                plugin = value;

                if (value == null)
                {
                    Clear();
                }
                else
                {
                    RenderPackage();
                }
            }
        }

        public void Clear()
        {
            DownloadCountTextBlock.Text = string.Empty;
            AuthorTextBlock.Text = string.Empty;
            VersionTextBlock.Text = string.Empty;
            DescriptionTextBlock.Text = string.Empty;

            DownloadCountPanel.Visibility = Visibility.Collapsed;
            AuthorPanel.Visibility = Visibility.Collapsed;
            VersionPanel.Visibility = Visibility.Collapsed;
            LocationPanel.Visibility = Visibility.Collapsed;
        }

        public void Initialize([NotNull] BasePluginDescriptor plugin)
        {
            Assert.ArgumentNotNull(plugin, nameof(plugin));

            Plugin = plugin;
        }

        private void RenderPackage()
        {
            if (Plugin == null)
            {
                Clear();
                return;
            }

            AuthorTextBlock.Text = Plugin.Author;
            VersionTextBlock.Text = Plugin.Version;
            DownloadCountTextBlock.Text = Plugin.DownloadCount;
            DescriptionTextBlock.Text = Plugin.Description;
            LocationTextBlock.Text = Plugin.Location;
            LocationTextBlock.ToolTip = Plugin.Location;

            AuthorPanel.Visibility = string.IsNullOrEmpty(Plugin.Author) ? Visibility.Collapsed : Visibility.Visible;
            VersionPanel.Visibility = string.IsNullOrEmpty(Plugin.Version) ? Visibility.Collapsed : Visibility.Visible;
            DownloadCountPanel.Visibility = Plugin.DownloadCount == "-1" ? Visibility.Collapsed : Visibility.Visible;
            LocationPanel.Visibility = string.IsNullOrEmpty(Plugin.Location) ? Visibility.Collapsed : Visibility.Visible;

            if (Plugin.IconUrl == null)
            {
                return;
            }

            var policy = new RequestCachePolicy(RequestCacheLevel.Default);
            try
            {
                var bitmapImage = new BitmapImage(Plugin.IconUrl, policy);
                IconImage.Source = bitmapImage;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }
    }
}
