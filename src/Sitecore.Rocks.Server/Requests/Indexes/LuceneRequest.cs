// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Indexes
{
    public class LuceneRequest
    {
        [NotNull]
        public static object Evaluate([NotNull] Data.Query.Query query, [NotNull] QueryContext contextNode, [NotNull] string literal)
        {
            Assert.ArgumentNotNull(query, nameof(query));
            Assert.ArgumentNotNull(contextNode, nameof(contextNode));

            var result = new List<QueryContext>();
            var indexName = "sitecore_" + (contextNode.GetQueryContextItem()?.Database.Name ?? "master" )+ "_index";

            ISearchIndex index;
            try
            {
                index = ContentSearchManager.GetIndex(indexName);
            }
            catch
            {
                return string.Empty;
            }

            if (index == null)
            {
                return string.Empty;
            }

            using (var context = index.CreateSearchContext())
            {
                List<SearchResultItem> hits;
                try
                {
                    hits = context.GetQueryable<SearchResultItem>().Where(item => item.Content.Contains(literal)).ToList();
                }
                catch (Exception e)
                {
                    Log.Error("Invalid lucene search query: " + literal, e, typeof(LuceneRequest));
                    return string.Empty;
                }

                foreach (var hit in hits)
                {
                    var item = hit.GetItem();
                    if (item == null)
                    {
                        continue;
                    }

                    var queryContext = new QueryContext(item.Database.DataManager, item.ID);

                    result.Add(queryContext);
                }
            }

            return result.ToArray();
        }

        [NotNull]
        public string GetIndexes()
        {
            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented,
                Indentation = 2
            };

            output.WriteStartElement("indexes");

            WriteIndexes(output);

            output.WriteEndElement();

            return writer.ToString();
        }

        public static List<SearchResultItem> PerformContentSearch(string databaseName, [NotNull] string queryText, [CanBeNull] Item root, int pageNumber)
        {
            var indexName = "sitecore_" + databaseName + "_index";

            ISearchIndex index;
            try
            {
                index = ContentSearchManager.GetIndex(indexName);
            }
            catch
            {
                return null;
            }

            if (index == null)
            {
                return null;
            }

            using (var context = index.CreateSearchContext())
            {
                try
                {
                    var hits = context.GetQueryable<SearchResultItem>().Where(item => item.Content.Contains(queryText));

                    if (root != null)
                    {
                        hits = hits.Where(item => item.Paths.Contains(root.ID));
                    }

                    hits = hits.Skip(pageNumber * 250).Take(250);

                    return hits.ToList();
                }
                catch (Exception e)
                {
                    Log.Error("Invalid lucene search query: " + queryText, e, typeof(LuceneRequest));
                    return null;
                }
            }
        }

        public static List<SearchResultItem> PerformMediaSearch([NotNull] string databaseName, [NotNull] string queryText, [CanBeNull] Item root, int pageNumber)
        {
            var indexName = "sitecore_" + databaseName + "_index";

            ISearchIndex index;
            try
            {
                index = ContentSearchManager.GetIndex(indexName);
            }
            catch
            {
                return null;
            }

            if (index == null)
            {
                return null;
            }

            using (var context = index.CreateSearchContext())
            {
                try
                {
                    var hits = context.GetQueryable<SearchResultItem>().Where(item => item.Content.Contains(queryText) && item.TemplateId != TemplateIDs.MainSection && item.TemplateId != TemplateIDs.Folder && item.TemplateId != TemplateIDs.MediaFolder && item.TemplateId != TemplateIDs.Node);

                    if (root != null)
                    {
                        hits = hits.Where(item => item.Paths.Contains(root.ID));
                    }

                    hits = hits.Skip(pageNumber * 250).Take(250);

                    return hits.ToList();
                }
                catch (Exception e)
                {
                    Log.Error("Invalid lucene search query: " + queryText, e, typeof(LuceneRequest));
                    return null;
                }
            }
        }

        [NotNull]
        public string Search([NotNull] string indexName, [NotNull] string fieldName, [NotNull] string search, [NotNull] string type, [NotNull] string pageOffset)
        {
            Assert.ArgumentNotNull(indexName, nameof(indexName));
            Assert.ArgumentNotNull(fieldName, nameof(fieldName));
            Assert.ArgumentNotNull(search, nameof(search));
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(pageOffset, nameof(pageOffset));

            ISearchIndex index;
            try
            {
                index = ContentSearchManager.GetIndex(indexName);
            }
            catch
            {
                return string.Empty;
            }

            if (index == null)
            {
                return string.Empty;
            }

            int offset;
            int.TryParse(pageOffset, out offset);

            using (var context = index.CreateSearchContext())
            {
                var searchResultItems = context.GetQueryable<SearchResultItem>().Where(item => item[fieldName].Contains(search)).Skip(offset).ToList();
                                                                                                                   
                var writer = new StringWriter();
                var output = new XmlTextWriter(writer)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 2
                };

                output.WriteStartElement("hits");

                WriteDocuments(output, searchResultItems, 0);

                output.WriteEndElement();

                return writer.ToString();
            }
        }

        private void WriteColumns([NotNull] XmlTextWriter output, [NotNull] List<string> columns)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(columns, nameof(columns));

            output.WriteStartElement("columns");

            foreach (var column in columns)
            {
                output.WriteStartElement("column");
                output.WriteAttributeString("name", column);
                output.WriteEndElement();
            }

            output.WriteEndElement();
        }

        private void WriteDocument([NotNull] XmlTextWriter output, [NotNull] List<string> columns, [NotNull] SearchResultItem doc)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(columns, nameof(columns));
            Assert.ArgumentNotNull(doc, nameof(doc));

            output.WriteStartElement("document");

            var names = new List<string>();

            foreach (var pair in doc.Fields)
            {
                var name = pair.Key;

                var index = 0;
                while (names.Contains(name))
                {
                    name = pair.Key + index;
                    index++;
                }

                names.Add(name);

                if (!columns.Contains(name))
                {
                    columns.Add(name);
                }

                output.WriteStartElement("field");
                output.WriteAttributeString("name", name);
                output.WriteCData(pair.Value?.ToString() ?? string.Empty);
                output.WriteEndElement();
            }

            output.WriteEndElement();
        }

        private void WriteDocuments([NotNull] XmlTextWriter output, [NotNull] List<SearchResultItem> documents, int total)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(documents, nameof(documents));

            var columns = new List<string>();

            output.WriteStartElement("documents");
            output.WriteAttributeString("count", documents.Count.ToString());
            output.WriteAttributeString("total", total.ToString());

            foreach (var document in documents)
            {
                WriteDocument(output, columns, document);
            }

            output.WriteEndElement();

            WriteColumns(output, columns);
        }

        private void WriteIndexes([NotNull] XmlTextWriter output)
        {
            Debug.ArgumentNotNull(output, nameof(output));

            if (!ContentSearchManager.Indexes.Any())
            {
                return;
            }

            foreach (var index in ContentSearchManager.Indexes)
            {
                output.WriteStartElement("index");

                output.WriteAttributeString("name", index.Name);
                output.WriteAttributeString("count", "0");

                output.WriteStartElement("fields");
                output.WriteEndElement();

                output.WriteEndElement();
            }
        }
    }
}
