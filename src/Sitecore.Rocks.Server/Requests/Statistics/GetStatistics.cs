// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Xml;

namespace Sitecore.Rocks.Server.Requests.Statistics
{
    public class GetStatistics
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

            output.WriteStartElement("statistics");

            foreach (var data in Sitecore.Diagnostics.Statistics.RenderingStatistics)
            {
                output.WriteStartElement("statistic");

                output.WriteAttributeString("name", data.TraceName);
                output.WriteAttributeString("site", data.SiteName);
                output.WriteAttributeString("rendercount", data.RenderCount.ToString());
                output.WriteAttributeString("usedcache", data.UsedCache.ToString());
                output.WriteAttributeString("averagetime", data.AverageTime.TotalMilliseconds.ToString());
                output.WriteAttributeString("averageitems", data.AverageItemsAccessed.ToString());
                output.WriteAttributeString("maxtime", data.MaxTime.TotalMilliseconds.ToString());
                output.WriteAttributeString("maxitemsaccessed", data.MaxItemsAccessed.ToString());
                output.WriteAttributeString("totaltime", data.TotalTime.TotalMilliseconds.ToString());
                output.WriteAttributeString("totalitems", data.TotalItemsAccessed.ToString());
                output.WriteAttributeString("lastrendered", DateUtil.ToIsoDate(data.LastRendered));

                output.WriteEndElement();
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
