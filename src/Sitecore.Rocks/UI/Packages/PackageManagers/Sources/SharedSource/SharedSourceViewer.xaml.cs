// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Packages.PackageManagers.Sources.SharedSource
{
    public partial class SharedSourceViewer
    {
        public SharedSourceViewer()
        {
            InitializeComponent();
        }

        private void Browse([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Browsers.Navigate("http://trac.sitecore.net/Index");
        }

        private void BrowseMarketplace([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Browsers.Navigate("http://marketplace.sitecore.net");
        }
    }
}
