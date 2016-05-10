// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Windows;
using NuGet;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Packages.PluginManagers.Descriptors
{
    public class PackagePluginDescriptor : BasePluginDescriptor
    {
        public PackagePluginDescriptor([NotNull] IPackageRepository repository, [NotNull] IPackage package)
        {
            Assert.ArgumentNotNull(repository, nameof(repository));
            Assert.ArgumentNotNull(package, nameof(package));

            Repository = repository;
            Package = package;

            Author = string.Join(",", Package.Authors);
            Copyright = Package.Copyright;
            Description = Package.Description;
            DownloadCount = Package.DownloadCount.ToString("#,##0");
            IconUrl = Package.IconUrl;
            LicenseUrl = Package.LicenseUrl;
            ReleaseNotes = Package.ReleaseNotes;
            RequireLicenseAcceptance = Package.RequireLicenseAcceptance;
            Summary = Package.Summary;
            Tags = Package.Tags;
            Title = Package.Title;
            Version = Package.Version.ToString();

            if (string.IsNullOrEmpty(Summary))
            {
                Summary = Description;
            }

            if (string.IsNullOrEmpty(Description))
            {
                Description = Summary;
            }

            var localPackage = package as LocalPackage;
            if (localPackage != null)
            {
                Location = Path.Combine(AppHost.Plugins.PackageFolder, localPackage.GetFullName());
            }
        }

        [NotNull]
        public IPackage Package { get; protected set; }

        [NotNull]
        public IPackageRepository Repository { get; protected set; }

        public override void Install(Action completed)
        {
            Assert.ArgumentNotNull(completed, nameof(completed));

            var packageManager = new PackageManager(Repository, AppHost.Plugins.PackageFolder);

            packageManager.Logger = new PackageLogger("Sitecore Rocks Plugin Install");
            AppHost.Output.Show();

            packageManager.PackageInstalled += delegate
            {
                AppHost.Extensibility.Reinitialize();
                AppHost.MessageBox("The plugin has been successfully installed.", "Plugins", MessageBoxButton.OK, MessageBoxImage.Information);
                completed();
            };

            packageManager.InstallPackage(Package, false, true);
        }

        public override void Uninstall(Action completed)
        {
            Assert.ArgumentNotNull(completed, nameof(completed));

            var packageManager = new PackageManager(Repository, AppHost.Plugins.PackageFolder);

            packageManager.Logger = new PackageLogger("Sitecore Rocks Plugin Uninstall");
            AppHost.Output.Show();

            packageManager.PackageUninstalled += delegate
            {
                AppHost.Extensibility.Reinitialize();
                AppHost.MessageBox("The plugin has been successfully uninstalled.", "Plugins", MessageBoxButton.OK, MessageBoxImage.Information);
                completed();
            };

            packageManager.UninstallPackage(Package, true);
        }

        public override void Update(Action completed)
        {
            Assert.ArgumentNotNull(completed, nameof(completed));

            var packageManager = new PackageManager(Repository, AppHost.Plugins.PackageFolder);

            packageManager.Logger = new PackageLogger("Sitecore Rocks Plugin Update");
            AppHost.Output.Show();

            packageManager.PackageInstalled += delegate
            {
                AppHost.Extensibility.Reinitialize();
                AppHost.MessageBox("The plugin has been successfully updated.", "Plugins", MessageBoxButton.OK, MessageBoxImage.Information);
                completed();
            };

            packageManager.UpdatePackage(Package, true, true);
        }
    }
}
