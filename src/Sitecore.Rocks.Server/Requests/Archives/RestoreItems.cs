// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Configuration;
using Sitecore.Data.Archiving;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Archives
{
    public class RestoreItems
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string archiveName, [NotNull] string items)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(archiveName, nameof(archiveName));
            Assert.ArgumentNotNull(items, nameof(items));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var archive = ArchiveManager.GetArchive(archiveName, database);

            var result = string.Empty;

            foreach (var id in items.Split('|'))
            {
                var guid = new Guid(id);

                if (!archive.RestoreItem(guid))
                {
                    result = "Failed to restore one or more items";
                }
            }

            return result;
        }
    }
}
