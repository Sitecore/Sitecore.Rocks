// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Caching;

namespace Sitecore.Rocks.Server.Requests.Caches
{
    public class ClearAll
    {
        [NotNull]
        public string Execute()
        {
            foreach (var cache in CacheManager.GetAllCaches())
            {
                cache.Clear();
            }

            return string.Empty;
        }
    }
}
