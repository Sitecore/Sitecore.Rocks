// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensions.XmlTextWriterExtensions;

namespace Sitecore.Rocks.Server.Requests.Items
{
    public class GetDependencies
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string itemsList)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(itemsList, nameof(itemsList));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var items = new List<Item>();
            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("items");
            foreach (var id in itemsList.Split('|'))
            {
                var item = database.GetItem(id);
                if (item == null)
                {
                    continue;
                }

                if (items.Any(i => i.ID == item.ID))
                {
                    continue;
                }

                WriteItem(output, items, item);

                FindDependencies(output, items, item);
            }

            output.WriteEndElement();

            return writer.ToString();
        }

        private void FindDependencies([NotNull] XmlTextWriter output, [NotNull] List<Item> items, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(item, nameof(item));

            foreach (var itemLink in Globals.LinkDatabase.GetReferences(item))
            {
                var targetItem = itemLink.GetTargetItem();
                if (targetItem == null)
                {
                    continue;
                }

                if (items.Any(i => i.ID == targetItem.ID))
                {
                    continue;
                }

                if (targetItem.TemplateID == TemplateIDs.Template)
                {
                    WriteItemTree(output, items, targetItem);
                }
                else
                {
                    WriteItem(output, items, targetItem);
                }
            }
        }

        private void WriteItem([NotNull] XmlTextWriter output, [NotNull] List<Item> items, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(item, nameof(item));

            output.WriteItemHeader(item);
            items.Add(item);
        }

        private void WriteItemTree([NotNull] XmlTextWriter output, [NotNull] List<Item> items, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(item, nameof(item));

            if (items.Any(i => i.ID == item.ID))
            {
                return;
            }

            WriteItem(output, items, item);

            foreach (Item child in item.Children)
            {
                WriteItemTree(output, items, child);
            }
        }
    }
}
