// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Caching;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Caches
{
    public class Clear
    {
        [NotNull]
        public string Execute([NotNull] string caches)
        {
            Assert.ArgumentNotNull(caches, nameof(caches));

            foreach (var cacheName in caches.Split('|'))
            {
                var cache = CacheManager.FindCacheByName(cacheName);
                if (cache == null)
                {
                    continue;
                }

                cache.Clear();
            }

            return string.Empty;
        }
    }
}
