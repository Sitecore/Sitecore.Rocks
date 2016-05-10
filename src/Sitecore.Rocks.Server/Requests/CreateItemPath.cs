// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests
{
    public class CreateItemPath
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string path)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(path, nameof(path));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var item = database.CreateItemPath(path);
            if (item == null)
            {
                return string.Empty;
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("path");

            var current = item;
            while (current != null)
            {
                output.WriteStartElement("item");

                output.WriteAttributeString("id", current.ID.ToString());
                output.WriteAttributeString("name", current.Name);
                output.WriteAttributeString("displayname", current.DisplayName);

                output.WriteEndElement();

                current = current.Parent;
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
