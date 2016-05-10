// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Jobs;

namespace Sitecore.Rocks.Server.Requests.Indexes
{
    public class OptimizeIndex : LuceneRequest
    {
        [NotNull]
        public string Execute([NotNull] string indexName)
        {
            Assert.ArgumentNotNull(indexName, nameof(indexName));

            BackgroundJob.Run("Optimize Search Index", "Indexing", () => RunOptimize(indexName));

            return string.Empty;
        }

        private void RunOptimize([NotNull] string indexName)
        {
            Debug.ArgumentNotNull(indexName, nameof(indexName));

            Optimize(indexName);
        }
    }
}
