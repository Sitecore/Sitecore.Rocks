// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Configuration;
using Sitecore.Data.Serialization;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Serialization
{
    public class SerializeDatabase
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

            var item = database.GetRootItem();
            if (item == null)
            {
                throw new Exception("Item not found");
            }

            Manager.DumpTree(item);

            return string.Empty;
        }
    }
}
