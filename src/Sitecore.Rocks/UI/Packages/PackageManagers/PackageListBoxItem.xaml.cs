// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Packages.PackageManagers.Sources;

namespace Sitecore.Rocks.UI.Packages.PackageManagers
{
    public partial class PackageListBoxItem
    {
        public PackageListBoxItem([NotNull] PackageInformation package, [NotNull] Action<PackageInformation> install)
        {
            Assert.ArgumentNotNull(package, nameof(package));
            Assert.ArgumentNotNull(install, nameof(install));

            InitializeComponent();

            Package = package;

            InstallButton.Click += (sender, args) => install(package);
            RenderPackage();
        }

        [NotNull]
        public PackageInformation Package { get; }

        public void SetSelected([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            ButtonPanel.Visibility = Visibility.Visible;
        }

        public void SetUnselected([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            ButtonPanel.Visibility = Visibility.Collapsed;
        }

        private void RenderPackage()
        {
            NameTextBlock.Text = Package.PackageName;
            ReadmeTextBlock.Text = Package.Readme;
        }
    }
}
