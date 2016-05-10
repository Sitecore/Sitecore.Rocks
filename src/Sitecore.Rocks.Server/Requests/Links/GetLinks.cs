// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.Search;
using Sitecore.Rocks.Server.Extensions.XmlTextWriterExtensions;

namespace Sitecore.Rocks.Server.Requests.Links
{
    public class GetLinks
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string id)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(id, nameof(id));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var item = database.GetItem(id);
            if (item == null)
            {
                throw new Exception("Item not found");
            }

            var categorizer = Factory.CreateObject("/sitecore/search/categorizer", true) as CategorizeResults.Categorizer;
            Assert.IsNotNull(categorizer, "categorizer");

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("links");

            output.WriteAttributeString("name", item.Name);

            output.WriteStartElement("references");

            var links = new List<string>();

            foreach (var itemLink in Globals.LinkDatabase.GetReferences(item))
            {
                var targetItem = itemLink.GetTargetItem();
                if (targetItem == null)
                {
                    continue;
                }

                var key = targetItem.Database.Name + targetItem.ID;

                if (links.Contains(key))
                {
                    continue;
                }

                links.Add(key);

                var category = categorizer.GetCategory(targetItem);

                output.WriteItemHeader(targetItem, category);
            }

            output.WriteEndElement();

            output.WriteStartElement("referrers");

            links.Clear();

            foreach (var itemLink in Globals.LinkDatabase.GetReferrers(item))
            {
                var sourceItem = itemLink.GetSourceItem();
                if (sourceItem == null)
                {
                    continue;
                }

                var key = sourceItem.Database.Name + sourceItem.ID;

                if (links.Contains(key))
                {
                    continue;
                }

                links.Add(key);

                var category = categorizer.GetCategory(sourceItem);

                output.WriteItemHeader(sourceItem, category);
            }

            output.WriteEndElement();

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
