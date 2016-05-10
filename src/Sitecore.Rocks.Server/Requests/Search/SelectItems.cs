// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensions.XmlTextWriterExtensions;

namespace Sitecore.Rocks.Server.Requests.Search
{
    public class SelectItems
    {
        [NotNull]
        public string Execute([NotNull] string queryText, [NotNull] string databaseName)
        {
            Assert.ArgumentNotNull(queryText, nameof(queryText));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                return string.Empty;
            }

            var selectedItems = database.SelectItems(queryText);

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("children");

            foreach (var child in selectedItems)
            {
                output.WriteItemHeader(child, string.Empty);
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
