// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.Packages.AnalyzePackage;
using Sitecore.Rocks.UI.Packages.InstallPackages.Dialogs;

namespace Sitecore.Rocks.UI.Packages.InstallPackages
{
    public partial class InstallPackageDialog
    {
        public InstallPackageDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            Loaded += ControlLoaded;
            Closed += WindowClosed;
        }

        [NotNull]
        public Site Site { get; private set; }

        protected bool IsClosed { get; set; }

        [NotNull]
        protected string License { get; set; }

        [NotNull]
        protected string ServerFileName { get; private set; }

        public void Initialize([NotNull] Site site, [NotNull] string serverFileName)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(serverFileName, nameof(serverFileName));

            Site = site;
            ServerFileName = serverFileName;
        }

        private void Analyze([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var dialog = new AnalyzePackageDialog();

            dialog.Initialize(Site, ServerFileName);

            AppHost.Shell.ShowDialog(dialog);
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                NameTextBlock.Text = root.GetElementValue("name");
                AuthorTextBlock.Text = root.GetElementValue("author");
                PublisherTextBlock.Text = root.GetElementValue("publisher");
                VersionTextBlock.Text = root.GetElementValue("version");
                ReadmeTextBlock.Text = root.GetElementValue("readme");
                License = root.GetElementValue("license");

                NamePanel.Visibility = string.IsNullOrEmpty(NameTextBlock.Text) ? Visibility.Collapsed : Visibility.Visible;
                AuthorPanel.Visibility = string.IsNullOrEmpty(AuthorTextBlock.Text) ? Visibility.Collapsed : Visibility.Visible;
                PublisherPanel.Visibility = string.IsNullOrEmpty(PublisherTextBlock.Text) ? Visibility.Collapsed : Visibility.Visible;
                VersionPanel.Visibility = string.IsNullOrEmpty(VersionTextBlock.Text) ? Visibility.Collapsed : Visibility.Visible;
            };

            Site.DataService.ExecuteAsync("Packages.GetPackageInformation", completed, ServerFileName);
        }

        private void CreateAntiPackage([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AntiPackageButton.IsEnabled = false;

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                AntiPackageButton.IsEnabled = true;

                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                AppHost.MessageBox(string.Format("The Anti Package has been built and is located at:\n\n{0}", response), Rocks.Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);
            };

            Site.DataService.ExecuteAsync("Packages.CreateAntiPackage", completed, ServerFileName);
        }

        private void Install([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!string.IsNullOrEmpty(License))
            {
                var d = new LicenseWindow();
                d.Initialize(License);
                if (AppHost.Shell.ShowDialog(d) != true)
                {
                    return;
                }
            }
            else if (AppHost.MessageBox("Are you sure you want to install the package?", Rocks.Resources.Confirmation, MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            InstallButton.IsEnabled = false;

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                InstallButton.IsEnabled = true;

                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                AppHost.MessageBox("The package has been installed.", Rocks.Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);

                if (IsClosed)
                {
                    return;
                }

                this.Close(true);
            };

            Site.DataService.ExecuteAsync("Packages.InstallPackage", completed, ServerFileName, string.Empty);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }

        private void WindowClosed([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            IsClosed = true;
        }
    }
}
