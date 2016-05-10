// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.Packages.InstallPackages;
using Sitecore.Rocks.UI.Repositories;

namespace Sitecore.Rocks.UI.Packages.PackageManagers.Sources.Repositories
{
    public class RepositorySource : IPackageSource
    {
        public RepositorySource([NotNull] RepositoryEntry repositoryEntry)
        {
            Assert.ArgumentNotNull(repositoryEntry, nameof(repositoryEntry));

            Repository = repositoryEntry;
        }

        [CanBeNull]
        public PackageListBox PackageListBox { get; set; }

        [NotNull]
        public RepositoryEntry Repository { get; }

        [CanBeNull]
        public Site Site { get; set; }

        public void ClearControl()
        {
            PackageListBox = null;
        }

        public FrameworkElement GetControl(Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            var result = PackageListBox;

            if (result == null)
            {
                result = new PackageListBox();

                PackageListBox = result;
                Site = site;

                result.ShowActionButton("Add File", AddFile);
            }

            Refresh();
            return result;
        }

        private void AddFile([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var dialog = new OpenFileDialog
            {
                Title = "Add File",
                CheckFileExists = true,
                DefaultExt = @".ps1",
                Filter = @"All files|*.*",
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            var source = dialog.FileName;
            var target = Path.Combine(Repository.Path, Path.GetFileName(source) ?? string.Empty);

            AppHost.Files.CreateDirectory(Path.GetDirectoryName(target) ?? string.Empty);
            AppHost.Files.Copy(source, target, true);

            Refresh();
        }

        private void Install([NotNull] PackageInformation package)
        {
            Debug.ArgumentNotNull(package, nameof(package));

            if (AppHost.MessageBox("The package will be uploaded to the web site before installing.\n\nAre you sure you want to continue?", Resources.Confirmation, MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            var fileName = package.LocalFileName;

            if (!DataService.CheckFileSize(fileName, package.Site.Connection))
            {
                return;
            }

            var contents = File.ReadAllBytes(fileName);

            var data = Convert.ToBase64String(contents);

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                package.ServerFileName = response;

                var packageListBox = PackageListBox;
                if (packageListBox != null)
                {
                    var packageManager = packageListBox.GetAncestor<PackageManagerDialog>();
                    if (packageManager != null)
                    {
                        packageManager.RefreshSiteRepositories();
                    }
                }

                var d = new InstallPackageDialog();
                d.Initialize(package.Site, package.ServerFileName);
                AppHost.Shell.ShowDialog(d);
            };

            package.Site.DataService.ExecuteAsync("Packages.Upload", completed, data, Path.GetFileName(fileName));
        }

        private void LoadPackages([NotNull] Site site, [NotNull] List<PackageInformation> packages, [NotNull] string folder)
        {
            Debug.ArgumentNotNull(site, nameof(site));
            Debug.ArgumentNotNull(packages, nameof(packages));
            Debug.ArgumentNotNull(folder, nameof(folder));

            foreach (var fileName in AppHost.Files.GetFiles(folder))
            {
                if (string.Compare(Path.GetExtension(fileName), @".zip", StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    continue;
                }

                try
                {
                    var packageAnalyzer = new PackageAnalyzer(fileName);

                    var package = new PackageInformation(site, packageAnalyzer);

                    packages.Add(package);
                }
                catch (Exception ex)
                {
                    AppHost.Output.LogException(ex);
                }
            }

            foreach (var subfolder in AppHost.Files.GetDirectories(folder))
            {
                LoadPackages(site, packages, subfolder);
            }
        }

        private void Refresh()
        {
            var packageListBox = PackageListBox;
            if (packageListBox == null)
            {
                return;
            }

            var site = Site;
            if (site == null)
            {
                return;
            }

            var packages = new List<PackageInformation>();
            try
            {
                LoadPackages(site, packages, Repository.Path);
            }
            catch (Exception ex)
            {
                AppHost.Output.LogException(ex);
            }

            packageListBox.RenderPackages(packages, Install);
        }
    }
}
