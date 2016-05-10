// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
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

            foreach (var cache in caches)
            {
                output.WriteStartElement("cache");

                output.WriteAttributeString("name", cache.Name);
                output.WriteAttributeString("size", cache.Size.ToString());
                output.WriteAttributeString("count", cache.Count.ToString());
                output.WriteAttributeString("maxsize", cache.MaxSize.ToString());
                output.WriteAttributeString("scavengable", cache.Scavengable ? "true" : "false");
                output.WriteAttributeString("enabled", cache.Enabled ? "true" : "false");
                output.WriteAttributeString("priority", cache.DefaultPriority.ToString());

                output.WriteEndElement();
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
