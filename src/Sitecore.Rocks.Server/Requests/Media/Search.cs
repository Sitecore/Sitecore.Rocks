// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensions.XmlTextWriterExtensions;
using Sitecore.Rocks.Server.Requests.Indexes;
using Sitecore.Search;

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

                root = database.GetItem(itemId);
                if (root == null)
                {
                    return string.Empty;
                }
            }

            var results = new List<Item>();

            var query = GetQuery(queryText, field, condition);

            using (new LongRunningOperationWatcher(250, "Search for '{0} query", queryText))
            {
                PerformSearch(results, query, queryText, root, pageNumber);
            }

            if (results.Count == 0)
            {
                return string.Empty;
            }

            return FormatResults(results);
        }

        [NotNull]
        private string FormatResults([NotNull] List<Item> results)
        {
            Debug.ArgumentNotNull(results, nameof(results));

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("hits");

            foreach (var item in results)
            {
                output.WriteItemHeader(item);
            }

            output.WriteEndElement();

            return writer.ToString();
        }

        [NotNull]
        private QueryBase GetQuery([NotNull] string queryText, [NotNull] string field, [NotNull] string condition)
        {
            Debug.ArgumentNotNull(queryText, nameof(queryText));
            Debug.ArgumentNotNull(field, nameof(field));
            Debug.ArgumentNotNull(condition, nameof(condition));

            if (string.IsNullOrEmpty(field) && string.IsNullOrEmpty(condition))
            {
                return new FullTextQuery(queryText);
            }

            var query = new CombinedQuery();

            if (!string.IsNullOrEmpty(field))
            {
                var occurance = QueryOccurance.Should;

                switch (condition.ToLowerInvariant())
                {
                    case "must":
                        occurance = QueryOccurance.Must;
                        break;
                    case "not":
                        occurance = QueryOccurance.MustNot;
                        break;
                }

                query.Add(new FieldQuery(field.ToLowerInvariant(), queryText), occurance);
            }

            return query;
        }

        private void PerformSearch([NotNull] List<Item> results, [NotNull] QueryBase query, [NotNull] string queryText, [CanBeNull] Item root, int pageNumber)
        {
            Debug.ArgumentNotNull(results, nameof(results));
            Debug.ArgumentNotNull(query, nameof(query));
            Debug.ArgumentNotNull(queryText, nameof(queryText));

            LuceneRequest.PerformMediaSearch(results, query, queryText, root, pageNumber);
        }
    }
}
