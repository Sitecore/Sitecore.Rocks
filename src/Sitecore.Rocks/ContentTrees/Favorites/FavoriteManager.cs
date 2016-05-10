// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications.Storages;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.ContentTrees.Favorites
{
    public static class FavoriteManager
    {
        private const string StorageKey = "Favorites";

        private static readonly List<Favorite> favorites = new List<Favorite>();

        static FavoriteManager()
        {
            Load();

            Notifications.ItemDeleted += ItemDeleted;
            Notifications.ItemRenamed += ItemRenamed;
            Notifications.SiteDeleted += SiteDeleted;
        }

        public static void Add([NotNull] Favorite favorite)
        {
            Assert.ArgumentNotNull(favorite, nameof(favorite));

            favorites.Add(favorite);
        }

        public static void Clear()
        {
            favorites.Clear();
            Save();
        }

        public static void Delete([NotNull] Favorite favorite)
        {
            Assert.ArgumentNotNull(favorite, nameof(favorite));

            favorites.Remove(favorite);
        }

        [NotNull]
        public static List<Favorite> GetFavorites()
        {
            return favorites;
        }

        public static void Save()
        {
            Storage.Delete(StorageKey);

            for (var n = 0; n < favorites.Count; n++)
            {
                var favorite = favorites[n];

                AppHost.Settings.Set(StorageKey, "site" + n, favorite.ItemVersionUri.ItemUri.Site.Name);
                AppHost.Settings.Set(StorageKey, "name" + n, favorite.Name);
                AppHost.Settings.Set(StorageKey, "database" + n, favorite.ItemVersionUri.ItemUri.DatabaseName);
                AppHost.Settings.Set(StorageKey, "itemid" + n, favorite.ItemVersionUri.ItemUri.ItemId.ToString());
                AppHost.Settings.Set(StorageKey, "language" + n, favorite.ItemVersionUri.Language);
                AppHost.Settings.Set(StorageKey, "version" + n, favorite.ItemVersionUri.Version.ToString());
                AppHost.Settings.Set(StorageKey, "fullpath" + n, favorite.FullPath);
                AppHost.Settings.Set(StorageKey, "iconpath" + n, favorite.Icon.IconPath);
            }
        }

        private static void ItemDeleted([NotNull] object sender, [NotNull] ItemUri itemUri)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));

            for (var n = favorites.Count - 1; n >= 0; n--)
            {
                var favorite = favorites[n];
                if (favorite.ItemVersionUri.ItemUri == itemUri)
                {
                    favorites.Remove(favorite);
                }
            }

            Save();
        }

        private static void ItemRenamed([NotNull] object sender, [NotNull] ItemUri itemUri, [NotNull] string newName)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(newName, nameof(newName));

            for (var n = favorites.Count - 1; n >= 0; n--)
            {
                var favorite = favorites[n];

                if (favorite.ItemVersionUri.ItemUri == itemUri)
                {
                    favorite.Name = newName;
                }
            }

            Save();
        }

        private static void Load()
        {
            favorites.Clear();

            var n = 0;

            while (n < 99)
            {
                var siteName = AppHost.Settings.Get(StorageKey, "site" + n, null) as string;
                if (string.IsNullOrEmpty(siteName))
                {
                    break;
                }

                var name = AppHost.Settings.Get(StorageKey, "name" + n, string.Empty) as string ?? string.Empty;
                var databaseName = AppHost.Settings.Get(StorageKey, "database" + n, string.Empty) as string ?? string.Empty;
                var itemId = AppHost.Settings.Get(StorageKey, "itemId" + n, string.Empty) as string ?? string.Empty;
                var language = AppHost.Settings.Get(StorageKey, "language" + n, string.Empty) as string ?? string.Empty;
                var version = AppHost.Settings.Get(StorageKey, "version" + n, string.Empty) as string ?? string.Empty;
                var fullpath = AppHost.Settings.Get(StorageKey, "fullpath" + n, string.Empty) as string ?? string.Empty;
                var iconPath = AppHost.Settings.Get(StorageKey, "iconpath" + n, string.Empty) as string ?? string.Empty;

                n++;

                var site = SiteManager.GetSite(siteName);
                if (site == null)
                {
                    continue;
                }

                Guid guid;
                if (!Guid.TryParse(itemId, out guid))
                {
                    continue;
                }

                int versionNumber;
                if (!int.TryParse(version, out versionNumber))
                {
                    continue;
                }

                var favorite = new Favorite
                {
                    Name = name,
                    ItemVersionUri = new ItemVersionUri(new ItemUri(new DatabaseUri(site, new DatabaseName(databaseName)), new ItemId(guid)), new Language(language), new Data.Version(versionNumber)),
                    FullPath = fullpath,
                    Icon = new Icon(site, iconPath)
                };

                favorites.Add(favorite);
            }
        }

        private static void SiteDeleted([NotNull] object sender, [NotNull] Site site)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(site, nameof(site));

            for (var n = favorites.Count - 1; n >= 0; n--)
            {
                var favorite = favorites[n];

                if (favorite.ItemVersionUri.Site == site)
                {
                    favorites.Remove(favorite);
                }
            }

            Save();
        }
    }
}
