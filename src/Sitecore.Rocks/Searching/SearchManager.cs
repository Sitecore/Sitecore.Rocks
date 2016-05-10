// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.ObjectModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications.Storages;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Searching
{
    public static class SearchManager
    {
        private const string StorageKey = "Searches";

        static SearchManager()
        {
            SavedSearches = new ObservableCollection<SavedSearch>();

            Load();

            Notifications.SiteDeleted += SiteDeleted;
        }

        public static ObservableCollection<SavedSearch> SavedSearches { get; set; }

        public static void Add([NotNull] SavedSearch savedSearch)
        {
            Assert.ArgumentNotNull(savedSearch, nameof(savedSearch));

            SavedSearches.Add(savedSearch);
        }

        public static void Delete([NotNull] SavedSearch savedSearch)
        {
            Assert.ArgumentNotNull(savedSearch, nameof(savedSearch));

            SavedSearches.Remove(savedSearch);
        }

        public static void Save()
        {
            Storage.Delete(StorageKey);

            for (var n = 0; n < SavedSearches.Count; n++)
            {
                var savedSearch = SavedSearches[n];

                AppHost.Settings.Set(StorageKey, "site" + n, savedSearch.Site.Name);
                AppHost.Settings.Set(StorageKey, "name" + n, savedSearch.Name);
                AppHost.Settings.Set(StorageKey, "querytext" + n, savedSearch.QueryText);
                AppHost.Settings.Set(StorageKey, "field" + n, savedSearch.Field);
            }
        }

        private static void Load()
        {
            SavedSearches.Clear();

            var n = 0;

            while (n < 99)
            {
                var siteName = AppHost.Settings.Get(StorageKey, "site" + n, null) as string;
                if (string.IsNullOrEmpty(siteName))
                {
                    break;
                }

                var name = AppHost.Settings.Get(StorageKey, "name" + n, string.Empty) as string ?? string.Empty;
                var queryText = AppHost.Settings.Get(StorageKey, "querytext" + n, string.Empty) as string ?? string.Empty;
                var field = AppHost.Settings.Get(StorageKey, "field" + n, string.Empty) as string ?? string.Empty;

                n++;

                var site = SiteManager.GetSite(siteName);
                if (site == null)
                {
                    continue;
                }

                var search = new SavedSearch
                {
                    Site = site,
                    Name = name,
                    QueryText = queryText,
                    Field = field
                };

                SavedSearches.Add(search);
            }
        }

        private static void SiteDeleted([NotNull] object sender, [NotNull] Site site)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(site, nameof(site));

            for (var n = SavedSearches.Count - 1; n >= 0; n--)
            {
                var search = SavedSearches[n];

                if (search.Site == site)
                {
                    SavedSearches.Remove(search);
                }
            }

            Save();
        }
    }
}
