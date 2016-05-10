// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications.Storages;
using Sitecore.Rocks.ContentTrees.Favorites;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices.Dialogs;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Net;
using Sitecore.Rocks.Sites.Connections;
using Sitecore.Rocks.UI;
using Sitecore.Rocks.UI.UpdateServerComponents;

namespace Sitecore.Rocks.Sites
{
    public static class SiteManager
    {
        private const string StorageKey = "Connections";

        private static readonly List<Site> sites = new List<Site>();

        static SiteManager()
        {
            Load();
        }

        [NotNull]
        public static IEnumerable<Site> Sites
        {
            get { return sites; }
        }

        public static void Add([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.IsFalse(sites.Contains(site), @"Site already added");

            sites.Add(site);
        }

        [Localizable(false), NotNull]
        public static Site CreateSite([NotNull] string hostName, [NotNull] string userName, [NotNull] string password, [NotNull] string dataServiceName, [NotNull] string webRootPath)
        {
            Assert.ArgumentNotNull(hostName, nameof(hostName));
            Assert.ArgumentNotNull(userName, nameof(userName));
            Assert.ArgumentNotNull(password, nameof(password));
            Assert.ArgumentNotNull(dataServiceName, nameof(dataServiceName));
            Assert.ArgumentNotNull(webRootPath, nameof(webRootPath));

            var site = FindSite(hostName, userName);
            if (site != null)
            {
                return site;
            }

            var connection = new Connection
            {
                UserName = userName,
                Password = password,
                HostName = hostName,
                DataServiceName = dataServiceName,
                WebRootPath = webRootPath,
            };

            connection.FileName = ConnectionManager.GetFileName(connection);

            ConnectionManager.Add(connection);
            ConnectionManager.Save();

            site = new Site(connection);
            Add(site);

            CreateSiteTreeViewItem(site, null);

            Notifications.RaiseSiteAdded(site, site);

            return site;
        }

        public static void Delete([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            sites.Remove(site);

            if (site == AppHost.Settings.ActiveDatabaseUri.Site)
            {
                AppHost.Settings.ActiveDatabaseUri = DatabaseUri.Empty;
            }
        }

        [CanBeNull]
        public static Site FindSite([NotNull] string server, [NotNull] string userName)
        {
            Assert.ArgumentNotNull(server, nameof(server));
            Assert.ArgumentNotNull(userName, nameof(userName));

            foreach (var site in sites)
            {
                if (string.Compare(site.HostName, server, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    continue;
                }

                if (string.Compare(site.UserName, userName, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return site;
                }
            }

            return null;
        }

        [CanBeNull]
        public static Site GetSite([NotNull] string name)
        {
            Assert.ArgumentNotNull(name, nameof(name));

            return sites.FirstOrDefault(site => string.Compare(site.Name, name, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        [NotNull, Obsolete(@"Use Sites property.")]
        public static IEnumerable<Site> GetSites()
        {
            return sites;
        }

        [CanBeNull]
        public static Site NewConnection()
        {
            return NewConnection(null);
        }

        [CanBeNull]
        public static Site NewConnection([CanBeNull] ConnectionFolderTreeViewItem parent)
        {
            return NewConnection("localhost", @"sitecore\admin", parent);
        }

        [CanBeNull]
        public static Site NewConnection([Localizable(false), NotNull] string hostName, [NotNull] string userName)
        {
            Assert.ArgumentNotNull(hostName, nameof(hostName));
            Assert.ArgumentNotNull(userName, nameof(userName));

            return NewConnection(hostName, userName, null);
        }

        [CanBeNull]
        public static Site NewConnection([Localizable(false), NotNull] string hostName, [NotNull] string userName, [CanBeNull] ConnectionFolderTreeViewItem parent)
        {
            Assert.ArgumentNotNull(hostName, nameof(hostName));
            Assert.ArgumentNotNull(userName, nameof(userName));

            var connection = new Connection
            {
                UserName = userName,
                Password = @"b",
                HostName = hostName,
            };

            var site = new Site(connection);

            var dialog = new SiteEditor();
            dialog.Load(site);
            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return null;
            }

            if (FindSite(site.HostName, site.UserName) == null)
            {
                if (parent != null)
                {
                    connection.FileName = ConnectionManager.GetFileName(connection, parent.Folder);
                }

                Add(site);

                ConnectionManager.Add(site.Connection);
                ConnectionManager.Save();

                CreateSiteTreeViewItem(site, parent);

                Notifications.RaiseSiteAdded(site, site);
            }

            var editor = dialog.DataServiceEditor as WebServiceSiteEditor;
            if (editor != null)
            {
                if (editor.AutomaticUpdate.IsChecked == true)
                {
                    UpdateServerComponentsDialog.AutomaticUpdate(site.DataService, site.Name, site.WebRootPath, site);
                }
            }

            return site;
        }

        public static void RefreshLocalSites()
        {
            ConnectionManager.RefreshLocalConnections();

            for (var index = sites.Count - 1; index >= 0; index--)
            {
                var site = sites[index];

                if (!ConnectionManager.Connections.Contains(site.Connection))
                {
                    sites.Remove(site);
                }
            }

            foreach (var connection in ConnectionManager.Connections)
            {
                var site = sites.FirstOrDefault(s => s.Connection == connection);

                if (site == null)
                {
                    sites.Add(new Site(connection));
                }
            }
        }

        [Obsolete]
        public static void Save()
        {
            ConnectionManager.Save();
        }

        internal static void LoadSites()
        {
        }

        private static void CreateSiteTreeViewItem([NotNull] Site site, [CanBeNull] ConnectionFolderTreeViewItem parent)
        {
            Debug.ArgumentNotNull(site, nameof(site));

            var activeContentTree = ActiveContext.ActiveContentTree;
            if (activeContentTree == null)
            {
                var s = site;

                ActiveContext.ActiveContentTreeChanged += delegate
                {
                    if (s != null)
                    {
                        CreateSiteTreeViewItem(s, parent);
                    }

                    s = null;
                };

                return;
            }

            var item = site.GetTreeViewItem();
            if (item == null)
            {
                return;
            }

            item.Text = site.Name;
            item.Items.Add(DummyTreeViewItem.Instance);

            if (parent != null)
            {
                parent.Items.Add(item);
                return;
            }

            var treeView = activeContentTree.ItemTreeView.TreeView;

            foreach (var i in treeView.Items)
            {
                var connections = i as ConnectionFolderTreeViewItem;

                if (connections != null)
                {
                    connections.Add(item);
                    return;
                }
            }

            treeView.Items.Add(item);
        }

        private static void Load()
        {
            var siteName = AppHost.Settings.Get(StorageKey, "site0", null) as string;
            if (!string.IsNullOrEmpty(siteName))
            {
                UpgradeFromRegistry();
            }

            ConnectionManager.Load();
            ConnectionManager.RefreshLocalConnections();

            sites.Clear();

            foreach (var connection in ConnectionManager.Connections.OrderBy(connection => connection.HostName).ThenBy(connection => connection.UserName))
            {
                var site = new Site(connection);
                Add(site);
            }
        }

        private static void UpgradeFromRegistry()
        {
            var n = 0;
            var blowFish = new BlowFish(BlowFish.CipherKey);

            while (n < 99)
            {
                var siteName = AppHost.Settings.Get(StorageKey, "site" + n, null) as string;
                if (string.IsNullOrEmpty(siteName))
                {
                    break;
                }

                var server = AppHost.Settings.Get(StorageKey, "server" + n, string.Empty) as string ?? string.Empty;
                var folder = AppHost.Settings.Get(StorageKey, "folder" + n, string.Empty) as string ?? string.Empty;
                var userName = AppHost.Settings.Get(StorageKey, "userName" + n, string.Empty) as string ?? string.Empty;
                var useWindowsAuth = AppHost.Settings.GetBool(StorageKey, "useWindowsAuth" + n, false);
                var password = AppHost.Settings.Get(StorageKey, "encryptedpassword" + n, string.Empty) as string ?? string.Empty;
                var dataServiceName = AppHost.Settings.Get(StorageKey, "dataservicename" + n, string.Empty) as string ?? string.Empty;

                if (!string.IsNullOrEmpty(password))
                {
                    password = blowFish.Decrypt_ECB(password);
                }
                else
                {
                    password = AppHost.Settings.Get(StorageKey, "password" + n, string.Empty) as string ?? string.Empty;
                }

                var connection = new Connection
                {
                    UserName = userName,
                    Password = password,
                    HostName = server,
                    DataServiceName = dataServiceName,
                    WebRootPath = folder,
                    Description = siteName,
                    UseWindowsAuth = useWindowsAuth
                };

                ConnectionManager.Add(connection);
                n++;
            }

            ConnectionManager.Save();
            ConnectionManager.Clear();

            FavoriteManager.Clear();
            Storage.Delete(StorageKey);
        }
    }
}
