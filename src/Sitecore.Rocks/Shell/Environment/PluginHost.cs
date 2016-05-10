// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Shell.Environment
{
    public class PluginHost
    {
        public delegate void ResolvePluginAssembliesEventHandler(IList<string> fileNames, bool includePackages, bool includeAssemblies, bool includeServerComponents);

        public delegate void UninstallPluginsEventHandler();

        [NotNull]
        public string PackageFolder
        {
            get { return Path.Combine(AppHost.User.UserFolder, @"Packages"); }
        }

        [NotNull]
        public string PluginFolder
        {
            get { return Path.Combine(AppHost.User.UserFolder, @"Plugins"); }
        }

        public virtual void CleanPackageFolder()
        {
            var packageFolder = AppHost.Plugins.PackageFolder;

            foreach (var folder in Directory.GetDirectories(packageFolder))
            {
                if (!Directory.GetFiles(folder, "*.nupkg").Any())
                {
                    try
                    {
                        AppHost.Files.DeleteFolder(folder);
                    }
                    catch
                    {
                        // silent
                    }
                }
            }
        }

        public virtual void CleanPluginFolder()
        {
            var pluginFolder = AppHost.Plugins.PluginFolder;

            foreach (var folder in Directory.GetDirectories(pluginFolder))
            {
                if (Directory.GetFiles(folder, "*.nupkg").Any())
                {
                    try
                    {
                        AppHost.Files.DeleteFolder(folder);
                    }
                    catch
                    {
                        // silent
                    }
                }
            }
        }

        public virtual void GetAssemblies([NotNull] List<string> fileNames, bool includePackages, bool includeAssemblies, bool includeServerComponents)
        {
            Assert.ArgumentNotNull(fileNames, nameof(fileNames));

            var handler = ResolvePluginAssemblies;
            if (handler != null)
            {
                handler(fileNames, includePackages, includeAssemblies, includeServerComponents);
            }
        }

        public virtual bool IsClientComponent([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            return fileName.IndexOf(Constants.SitecoreRocksServer, StringComparison.InvariantCultureIgnoreCase) < 0;
        }

        public virtual bool IsServerComponent([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            return fileName.IndexOf(Constants.SitecoreRocksServer, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        public event ResolvePluginAssembliesEventHandler ResolvePluginAssemblies;

        [CanBeNull]
        public virtual Assembly SafeLoadAssembly([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            try
            {
                return Assembly.LoadFrom(fileName);
            }
            catch (Exception ex)
            {
                AppHost.Output.LogException(ex);
            }

            return null;
        }

        public virtual void Uninstall()
        {
            var handler = UninstallHandlers;
            if (handler != null)
            {
                handler();
            }
        }

        public event UninstallPluginsEventHandler UninstallHandlers;
    }
}
