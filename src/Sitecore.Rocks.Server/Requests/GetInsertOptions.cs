// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Masters;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensions.XmlTextWriterExtensions;

namespace Sitecore.Rocks.Server.Requests
{
    public class GetInsertOptions
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

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("insertoptions");

            var insertOptions = Masters.GetMasters(item);

            foreach (var option in insertOptions)
            {
                if (option.TemplateID == TemplateIDs.CommandMaster)
                {
                    continue;
                }

                output.WriteItemHeader(option);
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
