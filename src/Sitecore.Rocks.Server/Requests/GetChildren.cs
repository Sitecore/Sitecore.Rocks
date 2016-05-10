// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensions.XmlTextWriterExtensions;

namespace Sitecore.Rocks.Server.Requests
{
    public class GetChildren
    {
        [NotNull]
        public string Execute([NotNull] string id, [NotNull] string databaseName)
        {
            Assert.ArgumentNotNull(id, nameof(id));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var item = database.GetItem(id);
            if (item == null)
            {
                return string.Empty;
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("children");

            foreach (Item child in item.Children)
            {
                output.WriteItemHeader(child, string.Empty);
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
