// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensions.XmlTextWriterExtensions;

namespace Sitecore.Rocks.Server.Requests.Items
{
    public class GetModifiedItems
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string fromDate, [NotNull] string toDate)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(fromDate, nameof(fromDate));
            Assert.ArgumentNotNull(toDate, nameof(toDate));

            var items = new List<Item>();
            var keys = new List<string>();

            if (!string.IsNullOrEmpty(databaseName))
            {
                var database = Factory.GetDatabase(databaseName);
                if (database == null)
                {
                    throw new Exception("Database not found");
                }

                GetItems(database, fromDate, toDate, items, keys);
            }
            else
            {
                foreach (var database in Factory.GetDatabases())
                {
                    if (database.Name == "web" || database.ReadOnly)
                    {
                        continue;
                    }

                    GetItems(database, fromDate, toDate, items, keys);
                }
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("items");

            foreach (var item in items)
            {
                output.WriteItemHeader(item);
            }

            output.WriteEndElement();

            return writer.ToString();
        }

        private void GetItems([NotNull] Database database, [NotNull] string fromDate, [NotNull] string toDate, [NotNull] List<Item> items, [NotNull] List<string> keys)
        {
            Debug.ArgumentNotNull(database, nameof(database));
            Debug.ArgumentNotNull(fromDate, nameof(fromDate));
            Debug.ArgumentNotNull(toDate, nameof(toDate));
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(keys, nameof(keys));

            var from = DateUtil.IsoDateToDateTime(fromDate, DateTime.UtcNow);
            var to = DateUtil.IsoDateToDateTime(toDate, DateTime.UtcNow);

            var history = HistoryManager.GetHistory(database, @from, to);

            foreach (var entry in history)
            {
                var item = database.GetItem(entry.ItemId);
                if (item == null)
                {
                    continue;
                }

                var key = item.Database.Name + item.ID;
                if (keys.Contains(key))
                {
                    continue;
                }

                items.Add(item);
                keys.Add(key);
            }
        }
    }
}
