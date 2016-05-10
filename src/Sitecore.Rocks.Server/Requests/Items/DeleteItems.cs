// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Extensions.StringExtensions;
using Sitecore.Rocks.Server.Pipelines.DeleteItems;

namespace Sitecore.Rocks.Server.Requests.Items
{
    public class DeleteItems
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string items, bool dryRun, bool quick)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var list = items.Split('|');
            var itemList = list.Where(id => !string.IsNullOrEmpty(id)).Select(database.GetItem).Where(item => item != null).ToList();
            var count = 0;

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented
            };

            output.WriteStartElement("items");

            if (!dryRun && quick)
            {
                DeleteQuickly(itemList);
                count = itemList.Count;
            }
            else
            {
                Process(output, itemList, dryRun, ref count);
            }

            output.WriteEndElement();

            var result = writer.ToString();

            if (result == "<items />")
            {
                result = string.Format("<items count=\"{0}\" />", count);
            }
            else
            {
                result = string.Format("<items count=\"{0}\">", count) + result.Mid(7);
            }

            return result;
        }

        private void DeleteQuickly([NotNull] List<Item> itemList)
        {
            foreach (var item in itemList)
            {
                if (Settings.RecycleBinActive)
                {
                    item.Recycle();
                }
                else
                {
                    item.Delete();
                }
            }
        }

        private void Process(XmlTextWriter output, [NotNull] IEnumerable<Item> items, bool dryRun, ref int count)
        {
            var paths = items.Select(i => i.Paths.Path).ToList();

            foreach (var item in items)
            {
                ProcessItem(output, item, paths, dryRun, ref count);

                if (!dryRun)
                {
                    if (Settings.RecycleBinActive)
                    {
                        item.Recycle();
                    }
                    else
                    {
                        item.Delete();
                    }
                }
            }
        }

        private void ProcessItem(XmlTextWriter output, Item item, IEnumerable<string> paths, bool dryRun, ref int count)
        {
            DeleteItemPipeline.Run().WithParameters(output, item, paths, dryRun);
            count++;

            foreach (Item child in item.Children)
            {
                ProcessItem(output, child, paths, dryRun, ref count);
            }
        }
    }
}
