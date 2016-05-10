// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Rocks.Server.Extensions.XmlTextWriterExtensions;

namespace Sitecore.Rocks.Server.Requests.Items
{
    public class GetItemHeaders
    {
        [NotNull]
        public string Execute([NotNull] string items)
        {
            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("items");

            foreach (var part in items.Split('|'))
            {
                var tuple = part.Split(',');

                var database = Factory.GetDatabase(tuple[0]);
                if (database == null)
                {
                    continue;
                }

                var item = database.GetItem(tuple[1]);
                if (item == null)
                {
                    continue;
                }

                output.WriteItemHeader(item, string.Empty);
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
