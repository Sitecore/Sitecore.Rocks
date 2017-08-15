// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Caching;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Caches
{
    public class Scavenge
    {
        [NotNull]
        public string Execute([NotNull] string caches)
        {
            Assert.ArgumentNotNull(caches, nameof(caches));

            foreach (var cacheName in caches.Split('|'))
            {
                foreach (var cacheInfo in CacheManager.GetAllCaches())
                {
                    if (cacheInfo.Name == cacheName)
                    {
                        cacheInfo.Scavenge();
                    }
                }
            }

            return string.Empty;
        }
    }
}
