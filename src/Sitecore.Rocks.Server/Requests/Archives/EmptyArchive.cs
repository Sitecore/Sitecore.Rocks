// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Configuration;
using Sitecore.Data.Archiving;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Archives
{
    public class EmptyArchive
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string archiveName)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(archiveName, nameof(archiveName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var archive = ArchiveManager.GetArchive(archiveName, database);

            archive.RemoveEntries();

            return string.Empty;
        }
    }
}
