// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Xml;
using Sitecore.Caching;
using Sitecore.Caching.Generics;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Extensions.StringExtensions;

namespace Sitecore.Rocks.Server.Requests.Caches
{
    public class GetCacheKeys
    {
        [NotNull]
        public string Execute([NotNull] string cacheName)
        {
            Assert.ArgumentNotNull(cacheName, nameof(cacheName));

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented,
                Indentation = 2
            };

            output.WriteStartElement("cacheKeys");

            var cache = CacheManager.GetNamedInstance(cacheName, 0, false);
            if (cache != null)
            {
                Write(output, cache);
            }

            output.WriteEndElement();

            return writer.ToString();
        }

        private void Write([NotNull] XmlTextWriter output, [NotNull] ICache cache)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(cache, nameof(cache));

            Database database = null;

            var n = cache.Name.IndexOf("[", StringComparison.Ordinal);
            if (n >= 0)
            {
                var databaseName = cache.Name.Left(n);

                try
                {
                    database = Factory.GetDatabase(databaseName);
                }
                catch
                {
                    database = null;
                }
            }

            //MethodInfo to gets the entry represented by key
            //Parameter: The cache key
            //Returns: the entry (or null)
            //public ICacheEntry<TKey> GetEntry(TKey key)
            var getEntryMethod = typeof(Cache<string>).GetMethod("GetEntry", new Type[] {typeof(string)});

            foreach (var cacheKey in cache.GetCacheKeys())
            {
                output.WriteStartElement("key");
                output.WriteAttributeString("key", cacheKey.ToString());
                output.WriteAttributeString("size", string.Empty);
                output.WriteAttributeString("lastAccessed", string.Empty);

                var pathValue = string.Empty;
                if (database != null)
                {
                    pathValue = GetDbPathValue(database, cacheKey);
                }

                if (!String.IsNullOrEmpty(pathValue))
                {
                    output.WriteValue(pathValue);
                }
                else
                {
                    try
                    {
                        var cacheEntry = getEntryMethod.Invoke(cache, new[] {cacheKey});
                        var cacheValue = cacheEntry.GetType().GetProperty("Data").GetValue(cacheEntry);
                        var cacheType = cacheEntry.GetType().GetProperty("Data").GetValue(cacheEntry).GetType();
                        if (cacheType == typeof(string))
                        {
                            output.WriteValue(cacheValue);
                        }

                        if (cacheType == typeof(ID))
                        {
                            output.WriteValue(cacheValue.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        Sitecore.Diagnostics.Log.Error("Could not get values for cache: "+ cache.Name, ex);
                    }
                }

                output.WriteEndElement();
            }
        }
        
        /// <summary>
        /// Returns path if key is ID and empty string if it is not
        /// </summary>
        /// <param name="database">Database where item is located</param>
        /// <param name="key">Cache key, that could be ID</param>
        /// <returns></returns>
        private string GetDbPathValue([NotNull] Database database, [NotNull] object key)
        {
            Debug.ArgumentNotNull(database, nameof(database));
            Debug.ArgumentNotNull(key, nameof(key));

            var id = key.ToString().Left(Constants.GuidLength);
            if (!ID.IsID(id))
            {
                return string.Empty;
            }

            var item = database.GetItem(id);
            if (item != null)
            {
                return item.Paths.Path;
            }

            return string.Empty;
        }
    }
}
