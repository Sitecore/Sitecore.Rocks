// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Caching;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Caches
{
    public class RemoveCacheKeys
    {
        [NotNull]
        public string Execute([NotNull] string cacheName, [NotNull] string cacheKeys)
        {
            Assert.ArgumentNotNull(cacheName, nameof(cacheName));
            Assert.ArgumentNotNull(cacheKeys, nameof(cacheKeys));

            var cache = CacheManager.FindCacheByName(cacheName);
            if (cache == null)
            {
                return string.Empty;
            }

            var keys = cache.GetCacheKeys();
            for (var index = keys.Count - 1; index >= 0; index--)
            {
                var cacheKey = keys[index];

                if (cacheKeys.Contains("|" + cacheKey + "|"))
                {
                    cache.Remove(cacheKey);
                }
            }

            return string.Empty;
        }
    }
}
