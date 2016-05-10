// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Items
{
    public class GetHistory
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string fromDate, [NotNull] string toDate, [NotNull] string actions, [NotNull] string userName)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(fromDate, nameof(fromDate));
            Assert.ArgumentNotNull(toDate, nameof(toDate));
            Assert.ArgumentNotNull(actions, nameof(actions));
            Assert.ArgumentNotNull(userName, nameof(userName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var from = DateUtil.IsoDateToDateTime(fromDate, DateTime.UtcNow);
            var to = DateUtil.IsoDateToDateTime(toDate, DateTime.UtcNow);

            var history = HistoryManager.GetHistory(database, @from, to);

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("history");

            foreach (var entry in history)
            {
                if (!string.IsNullOrEmpty(actions) && actions.IndexOf(entry.Action.ToString(), StringComparison.InvariantCultureIgnoreCase) < 0)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(userName) && string.Compare(userName, entry.UserName, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    continue;
                }

                output.WriteStartElement("entry");

                output.WriteAttributeString("action", entry.Action.ToString());
                output.WriteAttributeString("category", entry.Category.ToString());
                output.WriteAttributeString("created", DateUtil.ToIsoDate(entry.Created));
                output.WriteAttributeString("id", entry.ItemId.ToString());
                output.WriteAttributeString("language", entry.ItemLanguage.ToString());
                output.WriteAttributeString("version", entry.ItemVersion.ToString());
                output.WriteAttributeString("task", entry.TaskStack);
                output.WriteAttributeString("username", entry.UserName);
                output.WriteAttributeString("path", entry.ItemPath);

                output.WriteValue(entry.AdditionalInfo);

                output.WriteEndElement();
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
