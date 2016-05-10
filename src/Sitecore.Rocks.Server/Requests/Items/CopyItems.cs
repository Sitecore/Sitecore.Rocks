// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensions.XmlTextWriterExtensions;

namespace Sitecore.Rocks.Server.Requests.Items
{
    public class CopyItems
    {
        [NotNull]
        public string Execute([NotNull] string sourceItemIds, [NotNull] string itemId, [NotNull] string databaseName)
        {
            Assert.ArgumentNotNull(sourceItemIds, nameof(sourceItemIds));
            Assert.ArgumentNotNull(itemId, nameof(itemId));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var parent = database.Items[itemId];
            Debug.Assert(parent != null, "Parent item \"" + itemId + "\" not found.");

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("items");
            var list = sourceItemIds.Split(',');
            foreach (var id in list)
            {
                var source = database.GetItem(id);
                if (source == null)
                {
                    continue;
                }

                if (!source.Access.CanCopyTo(parent))
                {
                    continue;
                }

                var newName = source.Name;

                var count = 0;
                while (parent.Children[newName] != null)
                {
                    newName = source.Name + " - Copy";

                    if (count > 0)
                    {
                        newName += " (" + count + ")";
                    }

                    count++;
                }

                var copy = source.CopyTo(parent, newName);
                output.WriteItemHeader(copy);
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
