// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Reflection;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Shell
{
    public static class WebAdministration
    {
        private static bool _hasPermission = true;

        private static bool? _hasWebAdministrationFile;

        private static dynamic _serverManager;

        private static bool _serverManagerLoaded;

        public static bool CanAdminister
        {
            get
            {
                if (AppHost.Options.DisableIisIntegration)
                {
                    return false;
                }

                if (_hasWebAdministrationFile == null)
                {
                    _hasWebAdministrationFile = File.Exists(@"C:\windows\system32\inetsrv\Microsoft.Web.Administration.dll");
                    AppHost.Output.Log(@"Web Administration: C:\windows\system32\inetsrv\Microsoft.Web.Administration.dll = " + _hasWebAdministrationFile);
                }

                return _hasWebAdministrationFile == true;
            }
        }

        [CanBeNull]
        public static dynamic ServerManager
        {
            get
            {
                if (AppHost.Options.DisableIisIntegration)
                {
                    return null;
                }

                if (_serverManagerLoaded)
                {
                    return _serverManager;
                }

                _serverManagerLoaded = true;

                try
                {
                    var assembly = Assembly.LoadFrom(@"C:\windows\system32\inetsrv\Microsoft.Web.Administration.dll");
                    if (assembly == null)
                    {
                        AppHost.Output.Log(@"Web Administration: C:\windows\system32\inetsrv\Microsoft.Web.Administration.dll not found");
                        return null;
                    }

                    var type = assembly.GetType("Microsoft.Web.Administration.ServerManager");
                    if (type == null)
                    {
                        AppHost.Output.Log(@"Web Administration: Microsoft.Web.Administration.ServerManager not found");
                        return null;
                    }

                    _serverManager = Activator.CreateInstance(type);
                    if (_serverManager == null)
                    {
                        AppHost.Output.Log(@"Web Administration: Could not instantiate ServerManager");
                    }

                    return _serverManager;
                }
                catch (Exception ex)
                {
                    AppHost.Output.Log(@"Web Administration: Exception " + ex.Message);
                    return null;
                }
            }
        }

        [CanBeNull]
        public static dynamic GetWebApplicationFromWebRootPath([NotNull] string webRootPath)
        {
            Assert.ArgumentNotNull(webRootPath, nameof(webRootPath));

            if (!_hasPermission)
            {
                return null;
            }

            var manager = ServerManager;
            if (manager == null)
            {
                return null;
            }

            try
            {
                foreach (var site in manager.Sites)
                {
                    foreach (var application in site.Applications)
                    {
                        var virtualDirectories = application.VirtualDirectories;
                        if (virtualDirectories == null)
                        {
                            continue;
                        }

                        var virtualDirectory = virtualDirectories[@"/"];
                        if (virtualDirectory == null)
                        {
                            continue;
                        }

                        var path = virtualDirectory.PhysicalPath as string ?? string.Empty;

                        if (string.Compare(path, webRootPath, StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            return application;
                        }
                    }
                }
            }
            catch
            {
                _hasPermission = false;
                return null;
            }

            return null;
        }

        [NotNull]
        public static string GetWebRootPath([NotNull] dynamic website)
        {
            Assert.ArgumentNotNull(website, nameof(website));

            try
            {
                return website.Applications[0].VirtualDirectories["/"].PhysicalPath.ToString() as string ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        [CanBeNull]
        public static dynamic GetWebSiteFromWebRootPath([NotNull] string webRootPath)
        {
            Assert.ArgumentNotNull(webRootPath, nameof(webRootPath));

            if (!_hasPermission)
            {
                return null;
            }

            var manager = ServerManager;
            if (manager == null)
            {
                return null;
            }

            try
            {
                foreach (var site in manager.Sites)
                {
                    foreach (var application in site.Applications)
                    {
                        var virtualDirectory = application.VirtualDirectories[@"/"];
                        if (virtualDirectory == null)
                        {
                            continue;
                        }

                        var path = virtualDirectory.PhysicalPath as string ?? string.Empty;

                        if (string.Compare(path, webRootPath, StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            return site;
                        }
                    }
                }
            }
            catch
            {
                _hasPermission = false;
            }

            return null;
        }

        [NotNull]
        public static string GetWebSiteHostName([NotNull] dynamic website)
        {
            Assert.ArgumentNotNull(website, nameof(website));

            string host;
            string port;
            try
            {
                host = website.Bindings[0].Host.ToString() as string ?? string.Empty;
                port = website.Bindings[0].EndPoint.Port.ToString() as string ?? string.Empty;
            }
            catch
            {
                return "Unknown Site";
            }

            if (string.IsNullOrEmpty(host))
            {
                host = "localhost";
            }

            if (port == "80")
            {
                port = string.Empty;
            }
            else
            {
                port = ":" + port;
            }

            return host + port;
        }

        [NotNull]
        public static string GetWebSiteState([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            if (string.IsNullOrEmpty(site.WebRootPath))
            {
                return string.Empty;
            }

            if (!CanAdminister)
            {
                return string.Empty;
            }

            var app = GetWebSiteFromWebRootPath(site.WebRootPath);
            if (app == null)
            {
                return string.Empty;
            }

            int state;
            try
            {
                state = (int)app.State;
            }
            catch
            {
                return string.Empty;
            }

            switch (state)
            {
                case 0:
                    return "Starting";
                case 2:
                    return "Stopping";
                case 3:
                    return "Stopped";
                case 4:
                    return "Unknown";
            }

            return string.Empty;
        }

        public static void UnloadServerManager()
        {
            _serverManagerLoaded = false;
            _serverManager = null;
        }
    }
}
