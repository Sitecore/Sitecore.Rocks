// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Configuration;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests
{
    public class RebuildLinkDatabase
    {
        [NotNull]
        public string Execute([NotNull] string databaseName)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var database = Factory.GetDatabase(databaseName);
            Log.Audit(this, "Rebuild link database: {0}", database.Name);

            var linkDatabase = Globals.LinkDatabase;
            linkDatabase.Rebuild(database);

            return string.Empty;
        }
    }
}
