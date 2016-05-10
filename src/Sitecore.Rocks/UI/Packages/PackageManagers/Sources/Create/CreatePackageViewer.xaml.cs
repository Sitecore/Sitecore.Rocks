// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.Packages.PackageBuilders;

namespace Sitecore.Rocks.UI.Packages.PackageManagers.Sources.Create
{
    public partial class CreatePackageViewer
    {
        public CreatePackageViewer([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            InitializeComponent();

            Site = site;
        }

        [NotNull]
        public Site Site { get; }

        private void Create([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var window = this.GetAncestorOrSelf<Window>();
            if (window != null)
            {
                window.Close();
            }

            var packageBuilder = AppHost.OpenDocumentWindow<PackageBuilder>("Package");
            if (packageBuilder != null)
            {
                packageBuilder.Site = Site;
            }
        }
    }
}
