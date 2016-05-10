// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Projects
{
    public class GetItemsWithFilePaths
    {
        [NotNull]
        public string Execute([NotNull] string databaseName)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var items = database.SelectItems("fast://*[@Path != '' or @File != '' or @FileName != '' or @#File Name# != '']");

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("items");

            foreach (var item in items)
            {
                var path = item["Path"];

                if (string.IsNullOrEmpty(path))
                {
                    path = item["File"];
                }

                if (string.IsNullOrEmpty(path))
                {
                    path = item["FileName"];
                }

                if (string.IsNullOrEmpty(path))
                {
                    path = item["File Name"];
                }

                if (!string.IsNullOrEmpty(path))
                {
                    output.WriteStartElement("item");
                    output.WriteAttributeString("id", item.ID.ToString());
                    output.WriteAttributeString("path", path);
                    output.WriteEndElement();
                }
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
