// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using Microsoft.Win32;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.Packages.InstallPackages;

namespace Sitecore.Rocks.UI.Packages.PackageManagers.Sources.SitePackages
{
    [PackageSource("<Site>", "Packages", 1000)]
    public class SitePackagesSource : IPackageSource
    {
        [CanBeNull]
        public PackageListBox PackageListBox { get; set; }

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
            }
            result.ShowActionButton("Upload and Install", InstallPackage);

            LoadPackages(site);

            return result;
        }

        public void Refresh()
        {
            PackageListBox = null;
        }

        protected virtual bool CanShow([NotNull] XElement element)
        {
            Debug.ArgumentNotNull(element, nameof(element));

            return element.GetElementValue("name").IndexOf(@"Anti Package", StringComparison.InvariantCultureIgnoreCase) < 0;
        }

        private void Install([NotNull] PackageInformation package)
        {
            Debug.ArgumentNotNull(package, nameof(package));

            var d = new InstallPackageDialog();

            d.Initialize(package.Site, package.ServerFileName);

            AppHost.Shell.ShowDialog(d);
        }

        private void InstallPackage([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (Site == null)
            {
                return;
            }

            var dialog = new OpenFileDialog
            {
                Title = Resources.UploadViewer_Browse_Upload_Package_File,
                CheckFileExists = true,
                DefaultExt = @".zip",
                Filter = @"Packages|*.zip|All files|*.*"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

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

                d.Initialize(Site, response);

                AppHost.Shell.ShowDialog(d);
            };

            var contents = File.ReadAllBytes(dialog.FileName);

            var data = Convert.ToBase64String(contents);

            Site.DataService.ExecuteAsync("Packages.Upload", completed, data, Path.GetFileName(dialog.FileName));
        }

        private void LoadPackages([NotNull] Site site)
        {
            Debug.ArgumentNotNull(site, nameof(site));

            var packageListBox = PackageListBox;
            if (packageListBox == null)
            {
                return;
            }

            packageListBox.Clear();

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                packageListBox.Loading.HideLoading(packageListBox.PackageList);

                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                var packages = Parse(site, root).ToList();

                packageListBox.RenderPackages(packages, Install);
            };

            packageListBox.Loading.ShowLoading(packageListBox.PackageList);

            site.DataService.ExecuteAsync("Packages.GetPackageFolder", completed);
        }

        [NotNull]
        private IEnumerable<PackageInformation> Parse([NotNull] Site site, [NotNull] XElement root)
        {
            Debug.ArgumentNotNull(site, nameof(site));
            Debug.ArgumentNotNull(root, nameof(root));

            foreach (var element in root.Elements())
            {
                if (!CanShow(element))
                {
                    continue;
                }

                var fileName = element.GetElementValue("filename");

                var name = element.GetElementValue("name");
                if (string.IsNullOrEmpty(name))
                {
                    name = Path.GetFileNameWithoutExtension(fileName) ?? "[Unknown package]";
                }

                var packageInformation = new PackageInformation(site, name, element.GetElementValue("author"), element.GetElementValue("version"), element.GetElementValue("publisher"), element.GetElementValue("license"), element.GetElementValue("comment"), element.GetElementValue("readme"))
                {
                    ServerFileName = fileName
                };

                yield return packageInformation;
            }
        }
    }
}
