// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.UI.UpdateServerComponents.Updates;

namespace Sitecore.Rocks.UI.UpdateServerComponents
{
    [ExtensibilityInitialization(PreInit = "Clear")]
    public static class UpdateServerComponentsManager
    {
        private static readonly List<IServerComponentRemover> removers;

        private static readonly List<IServerComponentUpdater> updaters;

        static UpdateServerComponentsManager()
        {
            updaters = new List<IServerComponentUpdater>();
            removers = new List<IServerComponentRemover>();
        }

        [NotNull]
        public static IEnumerable<IServerComponentRemover> Removers
        {
            get { return removers; }
        }

        [NotNull]
        public static IEnumerable<IServerComponentUpdater> Updaters
        {
            get { return updaters; }
        }

        public static void AutomaticUpdate([NotNull] string webRootPath)
        {
            Assert.ArgumentNotNull(webRootPath, nameof(webRootPath));

            if (string.IsNullOrEmpty(webRootPath))
            {
                return;
            }

            var binFolder = Path.Combine(webRootPath, "bin");
            var sitecoreFolder = Path.Combine(webRootPath, "sitecore");

            if (!Directory.Exists(binFolder))
            {
                AppHost.MessageBox(string.Format(Resources.UpdateServerComponentsManager_AutomaticUpdate_, webRootPath), Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Directory.Exists(sitecoreFolder))
            {
                AppHost.MessageBox(string.Format(Resources.UpdateServerComponentsManager_AutomaticUpdate_1, webRootPath), Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var plugins = GetInstalledPlugins(webRootPath);

                var updates = new List<UpdateInfo>();

                foreach (var plugin in plugins)
                {
                    var serverFileName = Path.Combine(binFolder, Path.GetFileName(plugin.ServerFile));

                    var update = new UpdateInfo
                    {
                        Action = string.Empty,
                        IsChecked = true,
                        LocalVersion = plugin.Version,
                        Name = plugin.Name,
                        Plugin = plugin,
                        ServerComponent = null,
                        ServerVersion = string.Empty
                    };

                    if (!File.Exists(serverFileName))
                    {
                        updates.Add(update);
                        continue;
                    }

                    update.ServerVersion = FileVersionInfo.GetVersionInfo(serverFileName).FileVersion;

                    var sourceVersionInfo = new System.Version(update.LocalVersion);
                    var targetVersionInfo = new System.Version(update.ServerVersion);

                    if (targetVersionInfo < sourceVersionInfo)
                    {
                        updates.Add(update);
                    }
                }

                CopyFiles(updates, webRootPath);
            }
            catch (Exception ex)
            {
                Diagnostics.Trace.TraceError(ex.Message);
                AppHost.MessageBox(string.Format(Resources.UpdateServerComponentsManager_AutomaticUpdate_2, ex.Message), Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [UsedImplicitly]
        public static void Clear()
        {
            updaters.Clear();
            removers.Clear();
        }

        public static void CopyFiles([NotNull] IEnumerable<UpdateInfo> updates, [NotNull] string webRootPath)
        {
            Assert.ArgumentNotNull(updates, nameof(updates));
            Assert.ArgumentNotNull(webRootPath, nameof(webRootPath));

            if (!updates.Any())
            {
                return;
            }

            var binFolder = Path.Combine(webRootPath, @"bin");

            try
            {
                AppHost.Files.CreateDirectory(binFolder);
            }
            catch (UnauthorizedAccessException)
            {
                AppHost.MessageBox(Resources.UpdateServerComponentsManager_CopyFiles_Sorry__you_cannot_update_the_server_components__because_you_do_not_have_access_, Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            catch (DirectoryNotFoundException ex)
            {
                AppHost.MessageBox(ex.Message, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var server = updates.FirstOrDefault(u => u.Name == Constants.SitecoreRocksServer);
            if (server != null && server.IsChecked)
            {
                CopyFiles(server, webRootPath, binFolder);
            }

            foreach (var update in updates)
            {
                if (!update.IsChecked)
                {
                    continue;
                }

                if (update.Name == Constants.SitecoreRocksServer)
                {
                    continue;
                }

                CopyFiles(update, webRootPath, binFolder);
            }
        }

        [NotNull]
        public static IEnumerable<InstalledPluginInfo> GetInstalledPlugins([NotNull] string webRootPath)
        {
            Assert.ArgumentNotNull(webRootPath, nameof(webRootPath));

            var sitecoreVersion = GetSitecoreVersion(webRootPath);

            var result = new List<InstalledPluginInfo>();

            var kernelRuntimeVersion = RuntimeVersion.MaxValue;
            var kernelFileName = Path.Combine(webRootPath, @"bin\Sitecore.Kernel.dll");
            if (File.Exists(kernelFileName))
            {
                kernelRuntimeVersion = GetRuntimeVersion(kernelFileName);
            }

            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            var fileNames = AppHost.Files.GetFiles(folder, Constants.SitecoreRocksServer + @"*.dll");
            GetAssemblyFiles(result, fileNames, sitecoreVersion, kernelRuntimeVersion);

            var pluginFileNames = new List<string>();
            AppHost.Plugins.GetAssemblies(pluginFileNames, true, true, true);
            GetAssemblyFiles(result, pluginFileNames.Where(f => AppHost.Plugins.IsServerComponent(f)).ToList(), sitecoreVersion, kernelRuntimeVersion);

            return result;
        }

        public static void LoadType([NotNull] Type type, [NotNull] ServerComponentsRemoverAttribute attribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(attribute, nameof(attribute));

            IServerComponentRemover remover;
            try
            {
                var constructorInfo = type.GetConstructor(Type.EmptyTypes);
                if (constructorInfo == null)
                {
                    Diagnostics.Trace.TraceError("Failed to load server component remover");
                    return;
                }

                remover = constructorInfo.Invoke(null) as IServerComponentRemover;
            }
            catch (Exception ex)
            {
                Diagnostics.Trace.TraceError("Failed to load server component remover: " + ex.Message);
                return;
            }

            if (remover != null)
            {
                removers.Add(remover);
            }
        }

        public static void LoadType([NotNull] Type type, [NotNull] ServerComponentsUpdaterAttribute attribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(attribute, nameof(attribute));

            IServerComponentUpdater updater;
            try
            {
                var constructorInfo = type.GetConstructor(Type.EmptyTypes);
                if (constructorInfo == null)
                {
                    Diagnostics.Trace.TraceError("Failed to load server component updater");
                    return;
                }

                updater = constructorInfo.Invoke(null) as IServerComponentUpdater;
            }
            catch (Exception ex)
            {
                Diagnostics.Trace.TraceError("Failed to load server component updater: " + ex.Message);
                return;
            }

            if (updater != null)
            {
                updaters.Add(updater);
            }
        }

        private static void CopyFiles([NotNull] UpdateInfo update, [NotNull] string webRootPath, [NotNull] string binFolder)
        {
            Diagnostics.Debug.ArgumentNotNull(update, nameof(update));
            Diagnostics.Debug.ArgumentNotNull(webRootPath, nameof(webRootPath));
            Diagnostics.Debug.ArgumentNotNull(binFolder, nameof(binFolder));

            if (IsCustomInstallation(update, webRootPath))
            {
                return;
            }

            IO.File.CopyFile(binFolder, update.Plugin.ServerFile);
        }

        private static void GetAssemblyFiles([NotNull] List<InstalledPluginInfo> result, [NotNull] IEnumerable<string> fileNames, [NotNull] System.Version sitecoreVersion, [NotNull] RuntimeVersion kernelRuntimeVersion)
        {
            Diagnostics.Debug.ArgumentNotNull(result, nameof(result));
            Diagnostics.Debug.ArgumentNotNull(fileNames, nameof(fileNames));
            Diagnostics.Debug.ArgumentNotNull(sitecoreVersion, nameof(sitecoreVersion));
            Diagnostics.Debug.ArgumentNotNull(kernelRuntimeVersion, nameof(kernelRuntimeVersion));

            foreach (var fileName in fileNames)
            {
                var runtimeVersion = GetRuntimeVersion(fileName);
                if (kernelRuntimeVersion != RuntimeVersion.Empty && runtimeVersion > kernelRuntimeVersion)
                {
                    continue;
                }

                if (!IsSitecoreVersion(sitecoreVersion, fileName))
                {
                    continue;
                }

                var fileVersionInfo = FileVersionInfo.GetVersionInfo(fileName);

                var info = new InstalledPluginInfo
                {
                    ServerFile = fileName,
                    Name = Path.GetFileNameWithoutExtension(fileName) ?? string.Empty,
                    Version = fileVersionInfo.FileVersion,
                    RuntimeVersion = runtimeVersion
                };

                result.Add(info);
            }
        }

        [NotNull]
        private static System.Version GetFileVersion([NotNull] string fileName)
        {
            Diagnostics.Debug.ArgumentNotNull(fileName, nameof(fileName));

            try
            {
                var info = FileVersionInfo.GetVersionInfo(fileName);
                return new System.Version(info.FileMajorPart, info.FileMinorPart, info.FileBuildPart, info.FilePrivatePart);
            }
            catch (FileLoadException)
            {
                return new System.Version();
            }
            catch (BadImageFormatException)
            {
                return new System.Version();
            }
            catch (SecurityException)
            {
                return new System.Version();
            }
        }

        [NotNull]
        private static RuntimeVersion GetRuntimeVersion([NotNull] string fileName)
        {
            Diagnostics.Debug.ArgumentNotNull(fileName, nameof(fileName));

            try
            {
                var assembly = Assembly.ReflectionOnlyLoadFrom(fileName);

                var s = assembly.ImageRuntimeVersion;

                return RuntimeVersion.Parse(s);
            }
            catch (FileLoadException)
            {
                return RuntimeVersion.Empty;
            }
            catch (BadImageFormatException)
            {
                return RuntimeVersion.Empty;
            }
            catch (SecurityException)
            {
                return RuntimeVersion.Empty;
            }
        }

        [NotNull]
        private static System.Version GetSitecoreVersion([NotNull] string webRootPath)
        {
            Diagnostics.Debug.ArgumentNotNull(webRootPath, nameof(webRootPath));

            var result = new System.Version();

            var sitecoreVersionFile = Path.Combine(webRootPath, @"sitecore\shell\sitecore.version.xml");
            if (!File.Exists(sitecoreVersionFile))
            {
                var kernelFileName = Path.Combine(webRootPath, @"bin\Sitecore.Kernel.dll");
                return File.Exists(kernelFileName) ? GetFileVersion(kernelFileName) : result;
            }

            var root = File.ReadAllText(sitecoreVersionFile).ToXElement();
            if (root == null)
            {
                return result;
            }

            var versionElement = root.Element("version");
            if (versionElement == null)
            {
                return result;
            }

            var major = versionElement.GetElementValueInt("major");
            var minor = versionElement.GetElementValueInt("minor");
            var build = versionElement.GetElementValueInt("build");
            var revision = versionElement.GetElementValueInt("revision");

            return new System.Version(major, minor, build, revision);
        }

        [CanBeNull]
        private static IServerComponentUpdater GetUpdater([NotNull] UpdateServerComponentOptions options)
        {
            Diagnostics.Debug.ArgumentNotNull(options, nameof(options));

            foreach (var updater in Updaters)
            {
                if (updater.CanUpdate(options))
                {
                    return updater;
                }
            }

            return null;
        }

        [NotNull]
        private static UpdateServerComponentOptions GetUpdateServerComponentOptions([NotNull] UpdateInfo update, [NotNull] string webRootPath)
        {
            Diagnostics.Debug.ArgumentNotNull(update, nameof(update));
            Diagnostics.Debug.ArgumentNotNull(webRootPath, nameof(webRootPath));

            return new UpdateServerComponentOptions
            {
                WebSiteRootFolder = webRootPath,
                ComponentLocalFileName = update.Plugin.ServerFile,
                PluginName = update.Plugin.Name
            };
        }

        private static bool IsCustomInstallation([NotNull] UpdateInfo update, [NotNull] string webRootPath)
        {
            Diagnostics.Debug.ArgumentNotNull(update, nameof(update));
            Diagnostics.Debug.ArgumentNotNull(webRootPath, nameof(webRootPath));

            var options = GetUpdateServerComponentOptions(update, webRootPath);

            var updater = GetUpdater(options);
            if (updater == null)
            {
                return false;
            }

            return updater.Update(options);
        }

        private static bool IsSitecoreVersion([NotNull] System.Version sitecoreVersion, [NotNull] string fileName)
        {
            Diagnostics.Debug.ArgumentNotNull(sitecoreVersion, nameof(sitecoreVersion));
            Diagnostics.Debug.ArgumentNotNull(fileName, nameof(fileName));

            if (sitecoreVersion.Major == 0 && sitecoreVersion.Minor == 0)
            {
                return true;
            }

            IList<CustomAttributeData> attributes;
            try
            {
                var assembly = Assembly.LoadFrom(fileName);

                attributes = assembly.GetCustomAttributesData();
            }
            catch
            {
                return true;
            }

            if (attributes == null || attributes.Count == 0)
            {
                return true;
            }

            foreach (var attribute in attributes)
            {
                var name = attribute.ToString();
                if (name.IndexOf("SitecoreVersionAttribute", StringComparison.InvariantCultureIgnoreCase) < 0)
                {
                    continue;
                }

                var minVersionValue = string.Empty;
                var maxVersionValue = string.Empty;

                var args = attribute.ConstructorArguments;
                if (args.Count >= 1)
                {
                    minVersionValue = args[0].ToString().Trim('"');
                }

                if (args.Count >= 2)
                {
                    maxVersionValue = args[1].ToString().Trim('"');
                }

                if (!string.IsNullOrEmpty(minVersionValue))
                {
                    System.Version minVersion;
                    if (System.Version.TryParse(minVersionValue, out minVersion))
                    {
                        if (sitecoreVersion < minVersion)
                        {
                            return false;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(maxVersionValue))
                {
                    System.Version maxVersion;
                    if (System.Version.TryParse(maxVersionValue, out maxVersion))
                    {
                        if (sitecoreVersion > maxVersion)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
