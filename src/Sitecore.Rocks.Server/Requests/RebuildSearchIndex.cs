// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using Sitecore.Configuration;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Jobs;
using Sitecore.Search;

namespace Sitecore.Rocks.Server.Requests
{
    public class RebuildSearchIndex
    {
        [NotNull]
        public string Execute([NotNull] string databaseName)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            Log.Audit(this, "Rebuild link database: {0}", databaseName);
            BackgroundJob.Run("Rebuild Search Index", "Indexing", () => RebuildInxdexes(databaseName));

            return string.Empty;
        }

        private void RebuildInxdexes([NotNull] string databaseName)
        {
            Debug.ArgumentNotNull(databaseName, nameof(databaseName));

            IndexCustodian.RebuildAll();
        }
    }
}
