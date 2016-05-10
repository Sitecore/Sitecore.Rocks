// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Publishing;
using Sitecore.Rocks.Server.Extensions.XmlTextWriterExtensions;

namespace Sitecore.Rocks.Server.Requests.Publishing
{
    public class GetPublishingQueue
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, int pageNumber)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var publishQueue = PublishManager.GetPublishQueue(DateTime.MinValue, DateTime.MaxValue, database);

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("items");

            var count = 0;
            var start = pageNumber * 250;

            foreach (var id in publishQueue)
            {
                var item = database.GetItem(id);
                if (item == null)
                {
                    continue;
                }

                count++;

                if (count < start)
                {
                    continue;
                }

                if (count >= start + 250)
                {
                    break;
                }

                output.WriteItemHeader(item, string.Empty);
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
