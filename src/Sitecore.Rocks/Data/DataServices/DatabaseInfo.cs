// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Data.DataServices
{
    public class DatabaseInfo
    {
        public DatabaseInfo([NotNull] DatabaseName databaseName, [NotNull] string connectionString)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(connectionString, nameof(connectionString));

            DatabaseName = databaseName;
            ConnectionString = connectionString;
        }

        [NotNull]
        public string ConnectionString { get; private set; }

        [NotNull]
        public DatabaseName DatabaseName { get; private set; }
    }
}
