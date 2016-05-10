// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Packages.PackageManagers.Sources
{
    public partial class PackageInformationPanel
    {
        private PackageInformation package;

        public PackageInformationPanel()
        {
            InitializeComponent();
        }

        [CanBeNull]
        public PackageInformation Package
        {
            get { return package; }
            set
            {
                package = value;

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
            NameTextBlock.Text = string.Empty;
            AuthorTextBlock.Text = string.Empty;
            PublisherTextBlock.Text = string.Empty;
            VersionTextBlock.Text = string.Empty;
            ReadmeTextBlock.Text = string.Empty;

            NamePanel.Visibility = Visibility.Collapsed;
            AuthorPanel.Visibility = Visibility.Collapsed;
            PublisherPanel.Visibility = Visibility.Collapsed;
            VersionPanel.Visibility = Visibility.Collapsed;
        }

        public void Initialize([NotNull] PackageInformation package)
        {
            Assert.ArgumentNotNull(package, nameof(package));

            Package = package;
        }

        private void RenderPackage()
        {
            NameTextBlock.Text = Package.PackageName;
            AuthorTextBlock.Text = Package.Author;
            PublisherTextBlock.Text = Package.Publisher;
            VersionTextBlock.Text = Package.Version;
            ReadmeTextBlock.Text = Package.Readme;

            NamePanel.Visibility = Visibility.Visible;
            AuthorPanel.Visibility = string.IsNullOrEmpty(Package.Author) ? Visibility.Collapsed : Visibility.Visible;
            PublisherPanel.Visibility = string.IsNullOrEmpty(Package.Publisher) ? Visibility.Collapsed : Visibility.Visible;
            VersionPanel.Visibility = string.IsNullOrEmpty(Package.Version) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
