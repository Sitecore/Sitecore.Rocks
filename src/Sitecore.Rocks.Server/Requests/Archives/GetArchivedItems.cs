// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Archiving;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Archives
{
    public class GetArchivedItems
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string archiveName, int pageIndex)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(archiveName, nameof(archiveName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var archive = ArchiveManager.GetArchive(archiveName, database);

            var entries = archive.GetEntriesForUser(Context.User, pageIndex, 250);

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("archive");

            foreach (var entry in entries)
            {
                output.WriteStartElement("entry");

                output.WriteAttributeString("id", entry.ArchivalId.ToString("B").ToUpperInvariant());
                output.WriteAttributeString("datetime", DateUtil.ToIsoDate(entry.ArchiveLocalDate));
                output.WriteAttributeString("archivedby", entry.ArchivedBy);
                output.WriteAttributeString("name", entry.Name);
                output.WriteAttributeString("path", entry.OriginalLocation);

                output.WriteEndElement();
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
