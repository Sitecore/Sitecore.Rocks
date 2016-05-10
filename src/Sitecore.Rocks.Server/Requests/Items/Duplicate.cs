// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Items
{
    public class Duplicate
    {
        [NotNull]
        public string Execute([NotNull] string id, [NotNull] string name, [NotNull] string databaseName)
        {
            Assert.ArgumentNotNull(id, nameof(id));
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var item = database.GetItem(id);
            Debug.Assert(item != null, "Item \"" + id + "\" not found.");
            Debug.Assert(item.Access.CanDuplicate(), "You do not have permission to duplicate \"" + item.Name + "\".");

            var duplicate = item.Duplicate(name);

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("duplicate");

            output.WriteValue(duplicate.ID.ToString());

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
