// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.Sites.Connections
{
    public static class ConnectionManager
    {
        [NotNull]
        private static readonly List<Connection> connections = new List<Connection>();

        [NotNull]
        public static IEnumerable<Connection> Connections
        {
            get { return connections; }
        }

        public static void Add([NotNull] Connection connection)
        {
            Assert.ArgumentNotNull(connection, nameof(connection));

            connections.Add(connection);
        }

        public static void Clear()
        {
            connections.Clear();
        }

        public static bool Contains([NotNull] Connection connection)
        {
            Assert.ArgumentNotNull(connection, nameof(connection));

            return connections.Contains(connection);
        }

        [NotNull]
        public static string GetConnectionFolder()
        {
            var result = Path.Combine(AppHost.User.UserFolder, @"Connections");

            AppHost.Files.CreateDirectory(result);

            return result;
        }

        [NotNull]
        public static string GetFileName([NotNull] Connection connection)
        {
            Assert.ArgumentNotNull(connection, nameof(connection));

            return GetFileName(connection, GetConnectionFolder());
        }

        [NotNull]
        public static string GetFileName([NotNull] Connection connection, [NotNull] string folder)
        {
            Assert.ArgumentNotNull(connection, nameof(connection));
            Assert.ArgumentNotNull(folder, nameof(folder));

            var name = Regex.Replace(connection.HostName, @"\W", @"_");
            var index = 1;

            var fileName = Path.Combine(folder, name + @".xml");

            while (File.Exists(fileName))
            {
                fileName = Path.Combine(folder, name + @" (" + index + @").xml");
                index++;
            }

            connection.FileName = fileName;

            return fileName;
        }

        [NotNull]
        public static string GetLocalConnectionFolder()
        {
            var result = Path.Combine(AppHost.User.UserFolder, @"Connections\Local IIS Sites");

            AppHost.Files.CreateDirectory(result);

            return result;
        }

        public static void Load()
        {
            connections.Clear();

            Load(GetConnectionFolder());
        }

        public static bool RefreshLocalConnections()
        {
            WebAdministration.UnloadServerManager();

            dynamic serverManager;
            try
            {
                if (!WebAdministration.CanAdminister)
                {
                    return false;
                }

                serverManager = WebAdministration.ServerManager;
                if (serverManager == null)
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

            var modified = false;

            var folder = GetLocalConnectionFolder();
            AppHost.Files.CreateDirectory(folder);

            var localConnections = connections.Where(c => c.FileName.StartsWith(folder, StringComparison.InvariantCultureIgnoreCase)).ToList();

            try
            {
                foreach (var website in serverManager.Sites)
                {
                    string webRootPath;
                    string hostName;
                    try
                    {
                        hostName = WebAdministration.GetWebSiteHostName(website);
                        webRootPath = WebAdministration.GetWebRootPath(website);
                    }
                    catch
                    {
                        continue;
                    }

                    var list = connections.Where(c => string.Compare(c.HostName, hostName, StringComparison.InvariantCultureIgnoreCase) == 0).ToList();

                    if (list.Any())
                    {
                        foreach (var connection in list)
                        {
                            if (localConnections.Contains(connection) && connection.WebRootPath != webRootPath)
                            {
                                connection.WebRootPath = webRootPath;
                                modified = true;
                            }

                            localConnections.Remove(connection);
                        }

                        continue;
                    }

                    var newConnection = new Connection
                    {
                        UserName = "sitecore\\admin",
                        Password = "b",
                        HostName = hostName,
                        DataServiceName = "Hard Rock Web Service",
                        WebRootPath = webRootPath,
                        Description = hostName,
                        AutomaticallyUpdate = true
                    };

                    newConnection.FileName = GetFileName(newConnection, folder);

                    Add(newConnection);
                    modified = true;
                }
            }
            catch (Exception ex)
            {
                AppHost.Output.LogException(ex);
                return false;
            }

            foreach (var connection in localConnections)
            {
                Remove(connection);
                modified = true;
            }

            if (modified)
            {
                Save();
            }

            return modified;
        }

        public static void Remove([NotNull] Connection connection)
        {
            Assert.ArgumentNotNull(connection, nameof(connection));

            connections.Remove(connection);

            if (AppHost.Files.FileExists(connection.FileName))
            {
                AppHost.Files.Delete(connection.FileName);
            }
        }

        public static void Save()
        {
            foreach (var connection in connections)
            {
                var fileName = connection.FileName;

                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = GetFileName(connection);
                }

                AppHost.Files.CreateDirectory(Path.GetDirectoryName(fileName) ?? string.Empty);

                try
                {
                    using (var fileStream = new StreamWriter(fileName, false, Encoding.UTF8))
                    {
                        var output = new XmlTextWriter(fileStream)
                        {
                            Formatting = Formatting.Indented,
                            Indentation = 2
                        };

                        connection.Save(output);

                        output.Flush();
                        fileStream.Flush();
                    }
                }
                catch (IOException)
                {
                    AppHost.MessageBox(string.Format("Failed to write '{0}'", fileName), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        [NotNull]
        internal static IEnumerable<Connection> DeleteFolder([NotNull] string folder)
        {
            Debug.ArgumentNotNull(folder, nameof(folder));

            var result = new List<Connection>();

            for (var index = connections.Count - 1; index >= 0; index--)
            {
                var connection = connections[index];
                if (!connection.FileName.StartsWith(folder))
                {
                    continue;
                }

                connections.Remove(connection);
                result.Add(connection);
            }

            return result;
        }

        internal static void MoveFolder([NotNull] string oldFolder, [NotNull] string newFolder)
        {
            Debug.ArgumentNotNull(oldFolder, nameof(oldFolder));
            Debug.ArgumentNotNull(newFolder, nameof(newFolder));

            foreach (var connection in connections)
            {
                if (!connection.FileName.StartsWith(oldFolder, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var path = connection.FileName.Mid(oldFolder.Length);
                if (path.StartsWith("\\"))
                {
                    path = path.Mid(1);
                }

                connection.FileName = Path.Combine(newFolder, path);
            }

            Save();
        }

        private static void Load([NotNull] string folder)
        {
            Debug.ArgumentNotNull(folder, nameof(folder));

            foreach (var fileName in AppHost.Files.GetFiles(folder))
            {
                LoadConnection(fileName);
            }

            foreach (var subfolder in AppHost.Files.GetDirectories(folder))
            {
                Load(subfolder);
            }
        }

        private static void LoadConnection([NotNull] string fileName)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));

            XDocument doc;
            try
            {
                doc = XDocument.Load(fileName);
            }
            catch
            {
                Trace.TraceError("Failed to load connection file: " + fileName);
                return;
            }

            var root = doc.Root;
            if (root == null)
            {
                return;
            }

            var connection = new Connection
            {
                FileName = fileName
            };

            connection.Load(root);

            connections.Add(connection);
        }
    }
}
