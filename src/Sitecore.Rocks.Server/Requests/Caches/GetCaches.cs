// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using System.Xml;
using Sitecore.Caching;

namespace Sitecore.Rocks.Server.Requests.Caches
{
    public class GetCaches
    {
        [NotNull]
        public string Execute()
        {
            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented,
                Indentation = 2
            };

            output.WriteStartElement("caches");

            var caches = CacheManager.GetAllCaches();

            foreach (var cacheInfo in caches)
            {
                output.WriteStartElement("cache");

                output.WriteAttributeString("name", cacheInfo.Name);
                output.WriteAttributeString("size", cacheInfo.Size.ToString());
                output.WriteAttributeString("count", cacheInfo.Count.ToString());
                output.WriteAttributeString("maxsize", cacheInfo.MaxSize.ToString());
                output.WriteAttributeString("scavengable", cacheInfo.Scavengable ? "true" : "false");
                output.WriteAttributeString("enabled", cacheInfo.Enabled ? "true" : "false");
                output.WriteAttributeString("priority", string.Empty);

                output.WriteEndElement();
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
