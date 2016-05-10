// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.Packages.InstallPackages;

namespace Sitecore.Rocks.UI.Packages.PackageManagers.Sources.Upload
{
    public partial class UploadViewer
    {
        public UploadViewer([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            InitializeComponent();

            LabelTextBlock.Text = string.Format(Rocks.Resources.UploadViewer_UploadViewer_Select_a_package_file_to_upload_to___0___, site.Name);

            Site = site;

            EnableButtons();
        }

        [NotNull]
        public Site Site { get; }

        private void Browse([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var dialog = new OpenFileDialog
            {
                Title = Rocks.Resources.UploadViewer_Browse_Upload_Package_File,
                CheckFileExists = true,
                DefaultExt = @".zip",
                Filter = @"Packages|*.zip|All files|*.*"
            };

            if (!string.IsNullOrEmpty(LocationTextBox.Text))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(LocationTextBox.Text);
                dialog.FileName = Path.GetFileName(LocationTextBox.Text);
            }

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            LocationTextBox.Text = dialog.FileName;
        }

        private void EnableButtons([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
        }

        private void EnableButtons()
        {
            UploadButton.IsEnabled = !string.IsNullOrEmpty(LocationTextBox.Text) && File.Exists(LocationTextBox.Text);
        }

        private void InstallPackage([NotNull] string serverFileName)
        {
            Debug.ArgumentNotNull(serverFileName, nameof(serverFileName));

            var d = new InstallPackageDialog();

            d.Initialize(Site, serverFileName);

            AppHost.Shell.ShowDialog(d);
        }

        private void Upload([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var fileName = LocationTextBox.Text;

            if (!DataService.CheckFileSize(fileName, Site.Connection))
            {
                return;
            }

            var contents = File.ReadAllBytes(fileName);

            var data = Convert.ToBase64String(contents);

            UploadButton.IsEnabled = false;

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                UploadButton.IsEnabled = true;

                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var install = AppHost.MessageBox(Rocks.Resources.UploadViewer_Upload_The_package_has_been_uploaded_, Rocks.Resources.Information, MessageBoxButton.OKCancel, MessageBoxImage.Information);

                var packageManager = this.GetAncestor<PackageManagerDialog>();
                if (packageManager == null)
                {
                    return;
                }

                packageManager.RefreshSiteRepositories();
                packageManager.GoToSitePackages();

                if (install == MessageBoxResult.OK)
                {
                    InstallPackage(response);
                }
            };

            Site.DataService.ExecuteAsync("Packages.Upload", completed, data, Path.GetFileName(fileName));
        }
    }
}
