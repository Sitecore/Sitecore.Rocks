// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Data.DataServices
{
    public class DatabaseInfo
    {
        public DatabaseInfo([NotNull] DatabaseName databaseName)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            DatabaseName = databaseName;
        }

        [NotNull]
        public DatabaseName DatabaseName { get; private set; }
    }
}
