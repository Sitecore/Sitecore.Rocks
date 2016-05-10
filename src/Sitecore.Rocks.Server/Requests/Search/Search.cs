// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.Search;
using Sitecore.Rocks.Server.Extensions.XmlTextWriterExtensions;
using Sitecore.Rocks.Server.Requests.Indexes;
using Sitecore.Search;

namespace Sitecore.Rocks.Server.Requests.Search
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

            queryText = EscapeQueryText(queryText);

            var query = GetQuery(queryText, field, condition);

            var results = new SearchResultCollection();

            using (new LongRunningOperationWatcher(250, "Search for '{0}' query", queryText))
            {
                PerformSearch(results, query, queryText, root, pageNumber);
                Categorize(results);
            }

            if (results.Count == 0)
            {
                return string.Empty;
            }

            return FormatResults(results);
        }

        private void Categorize([NotNull] SearchResultCollection results)
        {
            Assert.ArgumentNotNull(results, nameof(results));

            var categorizer = Factory.CreateObject("/sitecore/search/categorizer", true) as CategorizeResults.Categorizer;
            Assert.IsNotNull(categorizer, "categorizer");

            foreach (var result in results)
            {
                var item = result.GetObject<Item>();
                if (item == null)
                {
                    results.AddResultToCategory(result, Texts.OTHER);
                    continue;
                }

                results.AddResultToCategory(result, categorizer.GetCategory(item));
            }
        }

        [NotNull]
        private string EscapeQueryText([NotNull] string queryText)
        {
            Debug.ArgumentNotNull(queryText, nameof(queryText));

            return queryText.Replace(@"\", @"\\");
        }

        [NotNull]
        private string FormatResults([NotNull] SearchResultCollection results)
        {
            Debug.ArgumentNotNull(results, nameof(results));

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("hits");

            foreach (var category in results.Categories)
            {
                foreach (var hit in category)
                {
                    var item = hit.GetObject<Item>();
                    if (item == null)
                    {
                        continue;
                    }

                    output.WriteItemHeader(item, category.Name);
                }
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

        private void PerformSearch([NotNull] SearchResultCollection results, [NotNull] QueryBase query, [NotNull] string queryText, [CanBeNull] Item root, int pageNumber)
        {
            Debug.ArgumentNotNull(results, nameof(results));
            Debug.ArgumentNotNull(query, nameof(query));
            Debug.ArgumentNotNull(queryText, nameof(queryText));

            LuceneRequest.PerformContentSearch(results, query, queryText, root, pageNumber);
        }
    }
}
