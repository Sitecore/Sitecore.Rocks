// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensions.XmlTextWriterExtensions;
using Sitecore.Rocks.Server.Requests.Indexes;

namespace Sitecore.Rocks.Server.Requests.Media
{
    public class Search
    {
        [NotNull]
        public string Execute([NotNull] string queryText, [NotNull] string field, [NotNull] string condition, [NotNull] string databaseName, [NotNull] string itemId, int pageNumber)
        {
            Assert.ArgumentNotNull(queryText, nameof(queryText));
            Assert.ArgumentNotNull(field, nameof(field));
            Assert.ArgumentNotNull(condition, nameof(condition));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(itemId, nameof(itemId));

            Item root = null;

            if (!string.IsNullOrEmpty(databaseName))
            {
                var database = Factory.GetDatabase(databaseName);
                if (database == null)
                {
                    return string.Empty;
                }

                if (!string.IsNullOrEmpty(itemId))
                {
                    root = database.GetItem(itemId);
                    if (root == null)
                    {
                        return string.Empty;
                    }
                }
            }

            queryText = EscapeQueryText(queryText);

            List<SearchResultItem> results;

            using (new LongRunningOperationWatcher(250, "Search for '{0}' query", queryText))
            {
                results = PerformSearch(databaseName, queryText, root, pageNumber);
            }

            if (results.Count == 0)
            {
                return string.Empty;
            }

            return FormatResults(results);
        }

        [NotNull]
        private string EscapeQueryText([NotNull] string queryText)
        {
            Debug.ArgumentNotNull(queryText, nameof(queryText));

            return queryText.Replace(@"\", @"\\");
        }

        [NotNull]
        private string FormatResults([NotNull] List<SearchResultItem> results)
        {
            Debug.ArgumentNotNull(results, nameof(results));

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("hits");

            foreach (var result in results)
            {
                var item = result.GetItem();
                if (item != null)
                {
                    output.WriteItemHeader(item);
                }
            }

            output.WriteEndElement();

            return writer.ToString();
        }

        [NotNull]
        private List<SearchResultItem> PerformSearch([NotNull] string databaseName, [NotNull] string queryText, [CanBeNull] Item root, int pageNumber)
        {
            return LuceneRequest.PerformMediaSearch(databaseName, queryText, root, pageNumber);
        }
    }
}
