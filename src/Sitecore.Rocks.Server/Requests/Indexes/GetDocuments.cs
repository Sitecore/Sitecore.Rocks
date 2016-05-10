// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Indexes
{
    public class GetDocuments : LuceneRequest
    {
        [NotNull]
        public string Execute([NotNull] string indexName, [NotNull] string fieldName, [NotNull] string term, [NotNull] string offset)
        {
            Assert.ArgumentNotNull(indexName, nameof(indexName));
            Assert.ArgumentNotNull(fieldName, nameof(fieldName));
            Assert.ArgumentNotNull(term, nameof(term));
            Assert.ArgumentNotNull(offset, nameof(offset));

            return GetDocuments(indexName, fieldName, term, offset);
        }
    }
}
