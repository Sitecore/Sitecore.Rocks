// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Items
{
    public class CopyTo
    {
        [NotNull]
        public string Execute([NotNull] string id, [NotNull] string newParent, [NotNull] string name, [NotNull] string databaseName)
        {
            Assert.ArgumentNotNull(id, nameof(id));
            Assert.ArgumentNotNull(newParent, nameof(newParent));
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

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

            var source = database.GetItem(id);
            Debug.Assert(source != null, "Item \"" + id + "\" not found.");

            var parent = database.Items[newParent];
            Debug.Assert(source != null, "Parent item \"" + newParent + "\" not found.");
            Debug.Assert(source.Access.CanCopyTo(parent), "You do not have permission to copy \"" + source.Name + "\" to \"" + parent.Name + "\".");

            var copy = source.CopyTo(parent, name);

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("copy");

            output.WriteValue(copy.ID.ToString());

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
