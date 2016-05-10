// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Configuration;
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

            var database = Factory.GetDatabase(databaseName);

            try
            {
                var index = SearchManager.SystemIndex;
                index.Rebuild();
            }
            catch (Exception exception)
            {
                Log.Error("Failed to rebuild system search index", exception, GetType());
            }

            for (var n = 0; n < database.Indexes.Count; n++)
            {
                try
                {
                    database.Indexes[n].Rebuild(database);
                    Log.Audit(this, "Rebuild search index: {0}", database.Name);
                }
                catch (Exception exception)
                {
                    Log.Error("Failed to rebuild search index", exception, GetType());
                }
            }
        }
    }
}
