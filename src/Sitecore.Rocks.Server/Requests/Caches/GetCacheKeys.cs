// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Caching;
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

            var cache = CacheManager.FindCacheByName(cacheName);
            if (cache != null)
            {
                Write(output, cache);
            }

            output.WriteEndElement();

            return writer.ToString();
        }

        private void Write([NotNull] XmlTextWriter output, [NotNull] Cache cache)
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

            foreach (var cacheKey in cache.GetCacheKeys())
            {
                var entry = cache.GetEntry(cacheKey, false);
                if (entry.Expired)
                {
                    continue;
                }

                var key = cacheKey.ToString();

                output.WriteStartElement("key");
                output.WriteAttributeString("key", key);
                output.WriteAttributeString("size", entry.DataLength.ToString());
                output.WriteAttributeString("lastAccessed", DateUtil.ToIsoDate(entry.Accessed));

                if (database != null)
                {
                    WriteValue(output, database, key);
                }

                output.WriteEndElement();
            }
        }

        private void WriteValue([NotNull] XmlTextWriter output, [NotNull] Database database, [NotNull] string key)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(database, nameof(database));
            Debug.ArgumentNotNull(key, nameof(key));

            var id = key.Left(Constants.GuidLength);
            if (!ID.IsID(id))
            {
                return;
            }

            var item = database.GetItem(id);
            if (item != null)
            {
                output.WriteValue(item.Paths.Path);
            }
        }
    }
}
