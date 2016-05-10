// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Indexes
{
    public class Search : LuceneRequest
    {
        [NotNull]
        public string Execute([NotNull] string indexName, [NotNull] string fieldName, [NotNull] string search, [NotNull] string type, [NotNull] string pageOffset)
        {
            Assert.ArgumentNotNull(indexName, nameof(indexName));
            Assert.ArgumentNotNull(fieldName, nameof(fieldName));
            Assert.ArgumentNotNull(search, nameof(search));
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(pageOffset, nameof(pageOffset));

            return Search(indexName, fieldName, search, type, pageOffset);
        }
    }
}
