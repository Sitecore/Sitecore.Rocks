// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

namespace Sitecore.Rocks.Server.Requests.Indexes
{
    public class GetTerms : LuceneRequest
    {
        [NotNull]
        public string Execute([NotNull] string indexName, [NotNull] string fieldName)
        {
            return GetTerms(indexName, fieldName);
        }
    }
}
