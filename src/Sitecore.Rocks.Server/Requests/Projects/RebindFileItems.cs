// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Extensions.StringExtensions;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Requests.Projects
{
    public class RebindFileItems
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string itemId)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(itemId, nameof(itemId));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var item = database.GetItem(itemId);
            if (item == null)
            {
                return string.Empty;
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("items");

            Process(output, item);

            output.WriteEndElement();

            return writer.ToString();
        }

        private void Process([NotNull] XmlTextWriter output, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            var path = item["Path"];

            if (!string.IsNullOrEmpty(path))
            {
                if (FileUtil.Exists(path))
                {
                    path = path.Replace("/", "\\");

                    if (path.StartsWith("\\"))
                    {
                        path = path.Mid(1);
                    }

                    output.WriteStartElement("item");
                    output.WriteAttributeString("id", item.ID.ToString());
                    output.WriteAttributeString("path", path);
                    output.WriteEndElement();
                }
            }

            foreach (Item child in item.Children)
            {
                Process(output, child);
            }
        }
    }
}
