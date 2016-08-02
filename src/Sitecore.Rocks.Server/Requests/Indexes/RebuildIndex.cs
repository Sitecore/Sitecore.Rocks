// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.ContentSearch;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Jobs;

namespace Sitecore.Rocks.Server.Requests.Indexes
{
    public class RebuildIndex
    {
        [NotNull]
        public string Execute([NotNull] string indexName)
        {
            Assert.ArgumentNotNull(indexName, nameof(indexName));

            BackgroundJob.Run("Rebuild Search Index", "Indexing", () => Rebuild(indexName));

            return string.Empty;
        }

        private void Rebuild([NotNull] string indexName)
        {
            Debug.ArgumentNotNull(indexName, nameof(indexName));

            var index = ContentSearchManager.GetIndex(indexName);
            index.Rebuild();
        }
    }
}
