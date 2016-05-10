// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Indexes
{
    public class GetDocument : LuceneRequest
    {
        [NotNull]
        public string Execute([NotNull] string indexName, [NotNull] string documentIndex)
        {
            Assert.ArgumentNotNull(indexName, nameof(indexName));
            Assert.ArgumentNotNull(documentIndex, nameof(documentIndex));

            return GetDocument(indexName, documentIndex);
        }
    }
}
