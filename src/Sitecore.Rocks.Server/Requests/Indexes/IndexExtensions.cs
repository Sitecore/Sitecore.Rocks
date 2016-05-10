// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Reflection;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Sitecore.Diagnostics;
using Sitecore.Search;

namespace Sitecore.Rocks.Server.Requests.Indexes
{
    public static class IndexExtensions
    {
        [CanBeNull]
        public static IndexWriter GetIndexWriter([NotNull] this IndexUpdateContext context)
        {
            Debug.ArgumentNotNull(context, nameof(context));

            var indexUpdateContextType = typeof(IndexUpdateContext);

            return indexUpdateContextType.InvokeMember("_writer", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, context, null) as IndexWriter;
        }

        [CanBeNull]
        public static IndexReader GetReader([NotNull] this Index index)
        {
            Debug.ArgumentNotNull(index, nameof(index));

            var methodInfo = typeof(Index).GetMethod("CreateReader", BindingFlags.NonPublic | BindingFlags.Instance);

            return methodInfo.Invoke(index, null) as IndexReader;
        }

        [CanBeNull]
        public static IndexSearcher GetSearcher([NotNull] this Index index)
        {
            Debug.ArgumentNotNull(index, nameof(index));

            var methodInfo = typeof(Index).GetMethod("CreateSearcher", BindingFlags.NonPublic | BindingFlags.Instance);

            return methodInfo.Invoke(index, new object[]
            {
                true
            }) as IndexSearcher;
        }
    }
}
