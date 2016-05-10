// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using NuGet;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Shell.Environment;
using Sitecore.Rocks.UI.Packages.PluginManagers.Galleries;

namespace Sitecore.Rocks.Extensions
{
    public static class PluginHostExtensions
    {
        public static void GetPluginAssemblies([NotNull] this PluginHost pluginHost, [NotNull] IList<string> fileNames, bool includePackages, bool includeAssemblies, bool includeServerComponents)
        {
            Assert.ArgumentNotNull(pluginHost, nameof(pluginHost));
            Assert.ArgumentNotNull(fileNames, nameof(fileNames));

            AppHost.Files.CreateDirectory(AppHost.Plugins.PluginFolder);
            AppHost.Files.CreateDirectory(AppHost.Plugins.PackageFolder);

            pluginHost.CleanPluginFolder();
            pluginHost.CleanPackageFolder();

            if (includeAssemblies)
            {
                pluginHost.GetPluginAssembliesFromPluginFolder(fileNames);
                pluginHost.GetPluginAssembliesFromPluginFolders(fileNames);
                pluginHost.GetPluginAssembliesFromCommandLine(fileNames, includeServerComponents);

                RemoveUninstalledAssemblies(fileNames);
            }

            if (includePackages)
            {
                pluginHost.GetPluginAssembliesFromPackages(fileNames);
            }
        }

        public static void GetPluginAssembliesFromCommandLine([NotNull] this PluginHost pluginHost, [NotNull] IList<string> fileNames, bool includeServerComponents)
        {
            Assert.ArgumentNotNull(pluginHost, nameof(pluginHost));
            Assert.ArgumentNotNull(fileNames, nameof(fileNames));

            var args = AppHost.Shell.CommandLineArgs;

            for (var index = 0; index < args.Length - 1; index++)
            {
                var part = args[index];

                if (string.Compare(part, @"/rocks.plugin", StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    continue;
                }

                var folder = args[index + 1];
                if (!AppHost.Files.FolderExists(folder))
                {
                    continue;
                }

                var assemblyFileNames = Directory.GetFiles(folder, "*.dll", SearchOption.AllDirectories);
                foreach (var assemblyFileName in assemblyFileNames)
                {
                    var f = Path.GetFileName(assemblyFileName);

                    if (!AppHost.Plugins.IsServerComponent(f) || includeServerComponents)
                    {
                        fileNames.Add(assemblyFileName);
                    }
                }
            }
        }

        public static void GetPluginAssembliesFromPackages([NotNull] this PluginHost pluginHost, [NotNull] IList<string> fileNames)
        {
            Assert.ArgumentNotNull(fileNames, nameof(fileNames));
            Assert.ArgumentNotNull(pluginHost, nameof(pluginHost));

            var repository = new SharedPackageRepository(AppHost.Plugins.PackageFolder);
            var packages = repository.GetPackages().ToList();

            foreach (var package in packages)
            {
                var localPackage = package as LocalPackage;
                if (localPackage == null)
                {
                    continue;
                }

                var packageFiles = localPackage.GetFiles().ToList();
                foreach (var packageFile in packageFiles)
                {
                    var physicalFile = packageFile as PhysicalPackageFile;
                    if (physicalFile == null)
                    {
                        continue;
                    }

                    var path = packageFile.Path;

                    var extension = Path.GetExtension(path);
                    if (string.Compare(extension, ".dll", StringComparison.InvariantCultureIgnoreCase) != 0)
                    {
                        continue;
                    }

                    if (!CheckFolders(path))
                    {
                        continue;
                    }

                    var fileName = Path.GetFileName(path);
                    if (fileNames.Any(f => string.Compare(Path.GetFileName(f), fileName, StringComparison.InvariantCultureIgnoreCase) == 0))
                    {
                        continue;
                    }

                    fileNames.Add(physicalFile.SourcePath);
                }
            }
        }

        public static void GetPluginAssembliesFromPluginFolder([NotNull] this PluginHost pluginHost, [NotNull] IList<string> fileNames)
        {
            Assert.ArgumentNotNull(pluginHost, nameof(pluginHost));
            Assert.ArgumentNotNull(fileNames, nameof(fileNames));

            GetAssembliesFromFolder(fileNames, AppHost.Plugins.PluginFolder);
        }

        public static void GetPluginAssembliesFromPluginFolders([NotNull] this PluginHost pluginHost, [NotNull] IList<string> fileNames)
        {
            Assert.ArgumentNotNull(pluginHost, nameof(pluginHost));
            Assert.ArgumentNotNull(fileNames, nameof(fileNames));

            foreach (var folderDescriptor in pluginHost.GetPluginFolders())
            {
                GetAssembliesFromFolder(fileNames, folderDescriptor.Location);
            }
        }

        [NotNull]
        public static IList<FolderDescriptor> GetPluginFolders([NotNull] this PluginHost pluginHost)
        {
            Assert.ArgumentNotNull(pluginHost, nameof(pluginHost));

            var result = new List<FolderDescriptor>();

            var source = AppHost.Settings.GetString("Plugins\\Folders", "Locations", string.Empty);

            foreach (var s in source.Split('|'))
            {
                if (!string.IsNullOrEmpty(s))
                {
                    result.Add(new FolderDescriptor(s));
                }
            }

            return result;
        }

        [NotNull]
        public static IList<GalleryDescriptor> GetPluginGalleries([NotNull] this PluginHost pluginHost)
        {
            Assert.ArgumentNotNull(pluginHost, nameof(pluginHost));

            var result = new List<GalleryDescriptor>();

            var source = AppHost.Settings.GetString("Plugins\\Gallery", "Sources", "Sitecore Rocks MyGet Gallery^https://www.myget.org/F/sitecore-rocks-v2/");

            foreach (var s in source.Split('|'))
            {
                var parts = s.Split('^');
                if (parts.Length != 2)
                {
                    continue;
                }

                result.Add(new GalleryDescriptor(parts[0], parts[1]));
            }

            return result;
        }

        public static void SetPluginFolders([NotNull] this PluginHost pluginHost, [NotNull] IEnumerable<FolderDescriptor> descriptors)
        {
            Assert.ArgumentNotNull(pluginHost, nameof(pluginHost));
            Assert.ArgumentNotNull(descriptors, nameof(descriptors));

            var s = string.Join("|", descriptors.Select(d => d.Location));

            AppHost.Settings.SetString("Plugins\\Folders", "Locations", s);
        }

        public static void SetPluginGalleries([NotNull] this PluginHost pluginHost, [NotNull] IEnumerable<GalleryDescriptor> descriptors)
        {
            Assert.ArgumentNotNull(pluginHost, nameof(pluginHost));
            Assert.ArgumentNotNull(descriptors, nameof(descriptors));

            var s = string.Join("|", descriptors.Select(d => d.Name + "^" + d.Location));

            AppHost.Settings.SetString("Plugins\\Gallery", "Sources", s);
        }

        public static void UninstallPlugins([NotNull] this PluginHost pluginHost)
        {
            Assert.ArgumentNotNull(pluginHost, nameof(pluginHost));

            var folder = AppHost.Plugins.PluginFolder;
            if (!AppHost.Files.FolderExists(folder))
            {
                return;
            }

            foreach (var source in Directory.GetFiles(folder, @"*.uninstall"))
            {
                try
                {
                    var target = AppHost.Files.ReadAllText(source);
                    File.Delete(source);

                    target = Path.Combine(AppHost.Plugins.PluginFolder, target);

                    if (AppHost.Files.FileExists(target))
                    {
                        AppHost.Files.Delete(target);
                    }
                    else if (AppHost.Files.FolderExists(target))
                    {
                        AppHost.Files.DeleteFolder(target);
                    }
                }
                catch (Exception ex)
                {
                    AppHost.MessageBox(string.Format(Resources.PluginManager_UninstallDeferred_, Path.GetFileNameWithoutExtension(source), ex.Message), Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private static bool CheckFolderName([NotNull] string subfolder)
        {
            Debug.ArgumentNotNull(subfolder, nameof(subfolder));

            if (subfolder.StartsWith("vs", StringComparison.InvariantCultureIgnoreCase))
            {
                return CheckVisualStudioVersion(subfolder);
            }

            if (subfolder.StartsWith("v", StringComparison.InvariantCultureIgnoreCase))
            {
                return CheckSitecoreRocksVersion(subfolder);
            }

            if (string.Compare(subfolder, "VisualStudio", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return AppHost.Shell.VisualStudioVersion.Major != 0;
            }

            if (string.Compare(subfolder, "Windows", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return AppHost.Shell.VisualStudioVersion.Major == 0;
            }

            return true;
        }

        private static bool CheckFolders([NotNull] string path)
        {
            Debug.ArgumentNotNull(path, nameof(path));

            foreach (var folderName in path.Split('\\'))
            {
                if (string.IsNullOrEmpty(folderName))
                {
                    continue;
                }

                if (!CheckFolderName(folderName))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CheckSitecoreRocksVersion([NotNull] string subfolder)
        {
            Debug.ArgumentNotNull(subfolder, nameof(subfolder));

            Version requiredVersion;
            if (!Version.TryParse(subfolder.Mid(1), out requiredVersion))
            {
                return true;
            }

            return IsVersionLessOrEqual(requiredVersion, AppHost.Shell.SitecoreRocksVersion);
        }

        private static bool CheckVisualStudioVersion([NotNull] string subfolder)
        {
            Debug.ArgumentNotNull(subfolder, nameof(subfolder));

            Version requiredVersion;
            if (!Version.TryParse(subfolder.Mid(2), out requiredVersion))
            {
                return true;
            }

            return IsVersionLessOrEqual(requiredVersion, AppHost.Shell.VisualStudioVersion);
        }

        private static void GetAssembliesFromFolder([NotNull] IList<string> fileNames, [NotNull] string folder)
        {
            Debug.ArgumentNotNull(fileNames, nameof(fileNames));
            Debug.ArgumentNotNull(folder, nameof(folder));

            if (!AppHost.Files.FolderExists(folder))
            {
                return;
            }

            foreach (var path in Directory.GetFiles(folder, "*.dll", SearchOption.AllDirectories))
            {
                var fileName = Path.GetFileName(path);
                var fileInfo = new FileInfo(path);
                if ((fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                if ((fileInfo.Attributes & FileAttributes.System) == FileAttributes.System)
                {
                    continue;
                }

                if (!CheckFolders(path))
                {
                    continue;
                }

                if (fileNames.Any(f => string.Compare(Path.GetFileName(f), fileName, StringComparison.InvariantCultureIgnoreCase) == 0))
                {
                    continue;
                }

                fileNames.Add(path);
            }
        }

        private static bool IsVersionLessOrEqual([NotNull] Version v1, [NotNull] Version v2)
        {
            Debug.ArgumentNotNull(v2, nameof(v2));
            Debug.ArgumentNotNull(v1, nameof(v1));

            if (v1.Major != v2.Major)
            {
                return v1.Major < v2.Major;
            }

            if (v1.Minor != v2.Minor)
            {
                return v1.Minor < v2.Minor;
            }

            if (v1.Build != v2.Build)
            {
                return v1.Build < v2.Build;
            }

            if (v1.Revision != v2.Revision)
            {
                return v1.Revision < v2.Revision;
            }

            return true;
        }

        private static void RemoveUninstalledAssemblies([NotNull] IList<string> fileNames)
        {
            Debug.ArgumentNotNull(fileNames, nameof(fileNames));

            var folder = AppHost.Plugins.PluginFolder;
            if (!AppHost.Files.FolderExists(folder))
            {
                return;
            }

            foreach (var source in Directory.GetFiles(folder, @"*.uninstall"))
            {
                var target = AppHost.Files.ReadAllText(source);

                fileNames.Remove(target);
            }
        }
    }
}
