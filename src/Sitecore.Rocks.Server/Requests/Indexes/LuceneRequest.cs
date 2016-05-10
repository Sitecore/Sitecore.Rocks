// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Search;

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

            using (var context = SearchManager.SystemIndex.CreateSearchContext())
            {
                SearchHits hits;

                try
                {
                    var searchContext = new SearchContext(Context.User);
                    hits = context.Search(literal, int.MaxValue, searchContext);
                }
                catch (Exception e)
                {
                    Log.Error("Invalid lucene search query: " + literal, e, typeof(LuceneRequest));
                    return null;
                }

                foreach (var hit in hits.FetchResults(0, query.Max))
                {
                    var item = hit.GetObject<Item>();
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
        public string GetDocument([NotNull] string indexName, [NotNull] string documentIndex)
        {
            Assert.ArgumentNotNull(indexName, nameof(indexName));
            Assert.ArgumentNotNull(documentIndex, nameof(documentIndex));

            int no;
            int.TryParse(documentIndex, out no);

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented,
                Indentation = 2
            };

            var index = SearchManager.GetIndex(indexName);

            var reader = index.GetReader();
            if (reader == null)
            {
                return string.Empty;
            }

            try
            {
                var document = reader.Document(no);

                output.WriteStartElement("document");

                WriteDocument(output, document);

                output.WriteEndElement();
            }
            finally
            {
                reader.Dispose();
            }

            return writer.ToString();
        }

        [NotNull]
        public string GetDocuments([NotNull] string indexName, [NotNull] string fieldName, [NotNull] string term, [NotNull] string offset)
        {
            Assert.ArgumentNotNull(indexName, nameof(indexName));
            Assert.ArgumentNotNull(fieldName, nameof(fieldName));
            Assert.ArgumentNotNull(term, nameof(term));
            Assert.ArgumentNotNull(offset, nameof(offset));

            int o;
            int.TryParse(offset, out o);

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented,
                Indentation = 2
            };

            var index = SearchManager.GetIndex(indexName);

            var reader = index.GetReader();
            if (reader == null)
            {
                return string.Empty;
            }

            var documents = new List<Document>();
            var count = 0;

            try
            {
                var terms = reader.Terms();

                while (terms.Next())
                {
                    var t = terms.Term;
                    if (t.Field != fieldName)
                    {
                        continue;
                    }

                    if (t.Text != term)
                    {
                        continue;
                    }

                    var termDocs = reader.TermDocs(t);

                    while (termDocs.Next())
                    {
                        if (count >= o && count < o + 100)
                        {
                            documents.Add(reader.Document(termDocs.Doc));
                        }

                        count++;
                    }
                }

                output.WriteStartElement("documents");
                WriteDocuments(output, documents, count);
                output.WriteEndElement();
            }
            finally
            {
                reader.Dispose();
            }

            return writer.ToString();
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

        [NotNull]
        public string GetTerms([NotNull] string indexName, [NotNull] string fieldName)
        {
            Assert.ArgumentNotNull(indexName, nameof(indexName));
            Assert.ArgumentNotNull(fieldName, nameof(fieldName));

            var index = SearchManager.GetIndex(indexName);
            if (index == null)
            {
                return string.Empty;
            }

            var reader = index.GetReader();
            if (reader == null)
            {
                return string.Empty;
            }

            try
            {
                var writer = new StringWriter();

                var output = new XmlTextWriter(writer)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 2
                };

                output.WriteStartElement("terms");

                var termEnumerator = reader.Terms();
                while (termEnumerator.Next())
                {
                    var term = termEnumerator.Term;
                    if (term.Field != fieldName)
                    {
                        continue;
                    }

                    output.WriteStartElement("term");
                    output.WriteAttributeString("documents", reader.DocFreq(term).ToString());
                    output.WriteCData(term.Text);
                    output.WriteEndElement();
                }

                output.WriteEndElement();

                return writer.ToString();
            }
            finally
            {
                reader.Dispose();
            }
        }

        public void Optimize([NotNull] string indexName)
        {
            Debug.ArgumentNotNull(indexName, nameof(indexName));

            var index = SearchManager.GetIndex(indexName);

            using (var context = index.CreateUpdateContext())
            {
                var writer = context.GetIndexWriter();
                if (writer != null)
                {
                    writer.Optimize();
                }

                context.Commit();
            }
        }

        public static void PerformContentSearch([NotNull] SearchResultCollection results, [NotNull] QueryBase query, [NotNull] string queryText, [CanBeNull] Item root, int pageNumber)
        {
            Debug.ArgumentNotNull(results, nameof(results));
            Debug.ArgumentNotNull(query, nameof(query));
            Debug.ArgumentNotNull(queryText, nameof(queryText));

            using (var context = SearchManager.SystemIndex.CreateSearchContext())
            {
                SearchHits hits;

                try
                {
                    if (root != null)
                    {
                        hits = context.Search(query, int.MaxValue, new SearchContext(Context.User, root));
                    }
                    else
                    {
                        var searchContext = new SearchContext(Context.User);

                        hits = context.Search(query, int.MaxValue, searchContext);
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Invalid lucene search query: " + queryText, e, typeof(LuceneRequest));
                    return;
                }

                foreach (var hit in hits.FetchResults(pageNumber * 250, (pageNumber + 1) * 250 - 1))
                {
                    results.AddResult(hit);
                }
            }
        }

        public static void PerformMediaSearch([NotNull] List<Item> results, [NotNull] QueryBase query, [NotNull] string queryText, [CanBeNull] Item root, int pageNumber)
        {
            Debug.ArgumentNotNull(results, nameof(results));
            Debug.ArgumentNotNull(query, nameof(query));
            Debug.ArgumentNotNull(queryText, nameof(queryText));

            using (var context = SearchManager.SystemIndex.CreateSearchContext())
            {
                SearchHits hits;

                try
                {
                    if (root != null)
                    {
                        hits = context.Search(query, int.MaxValue, new SearchContext(Context.User, root));
                    }
                    else
                    {
                        hits = context.Search(query, int.MaxValue, new SearchContext(Context.User));
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Invalid lucene search query: " + queryText, e, typeof(LuceneRequest));
                    return;
                }

                var start = pageNumber * 250;
                var end = start + 250 - 1;

                do
                {
                    foreach (var hit in hits.FetchResults(start, end))
                    {
                        var item = hit.GetObject<Item>();
                        if (item == null)
                        {
                            continue;
                        }

                        if (item.TemplateID == TemplateIDs.MainSection || item.TemplateID == TemplateIDs.Folder || item.TemplateID == TemplateIDs.MediaFolder || item.TemplateID == TemplateIDs.Node)
                        {
                            continue;
                        }

                        results.Add(item);
                    }

                    start = end + 1;
                    end = start + 250 - results.Count;

                    if (start >= hits.Length)
                    {
                        break;
                    }
                }
                while (results.Count < 250);
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

            var index = SearchManager.GetIndex(indexName);

            var searcher = index.GetSearcher();
            if (searcher == null)
            {
                return string.Empty;
            }

            int offset;
            int.TryParse(pageOffset, out offset);

            TopDocs hits = null;

            try
            {
                switch (type)
                {
                    case "QueryParser":
                        hits = QueryParserSearch(searcher, fieldName, search);
                        break;
                    case "TermQuery":
                        hits = TermSearch(searcher, fieldName, search);
                        break;
                    case "PrefixQuery":
                        hits = PrefixSearch(searcher, fieldName, search);
                        break;
                    case "WildCardQuery":
                        hits = WildCardSearch(searcher, fieldName, search);
                        break;
                }
            }
            catch
            {
                return string.Empty;
            }

            if (hits == null)
            {
                return string.Empty;
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented,
                Indentation = 2
            };

            output.WriteStartElement("hits");

            WriteDocuments(output, searcher, hits, offset);

            output.WriteEndElement();

            return writer.ToString();
        }

        [CanBeNull]
        private SearchConfiguration GetConfiguration()
        {
            var searchManagerType = typeof(SearchManager);

            return searchManagerType.InvokeMember("_configuration", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField, null, null, null) as SearchConfiguration;
        }

        [CanBeNull]
        private IndexReader GetReader([NotNull] Index index)
        {
            Debug.ArgumentNotNull(index, nameof(index));

            var methodInfo = typeof(Index).GetMethod("CreateReader", BindingFlags.NonPublic | BindingFlags.Instance);

            return methodInfo.Invoke(index, null) as IndexReader;
        }

        [CanBeNull]
        private TopDocs PrefixSearch([NotNull] IndexSearcher searcher, [NotNull] string field, [NotNull] string searchTerm)
        {
            Debug.ArgumentNotNull(searcher, nameof(searcher));
            Debug.ArgumentNotNull(field, nameof(field));
            Debug.ArgumentNotNull(searchTerm, nameof(searchTerm));

            var term = new Term(field, searchTerm);
            var query = new PrefixQuery(term);

            return searcher.Search(query, int.MaxValue);
        }

        [CanBeNull]
        private TopDocs QueryParserSearch([NotNull] IndexSearcher searcher, [NotNull] string field, [NotNull] string searchTerm)
        {
            Debug.ArgumentNotNull(searcher, nameof(searcher));
            Debug.ArgumentNotNull(field, nameof(field));
            Debug.ArgumentNotNull(searchTerm, nameof(searchTerm));

            var parser = new Lucene.Net.QueryParsers.QueryParser(Lucene.Net.Util.Version.LUCENE_30, field, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30));
            var query = parser.Parse(Lucene.Net.QueryParsers.QueryParser.Escape(searchTerm));

            return searcher.Search(query, int.MaxValue);
        }

        [CanBeNull]
        private TopDocs TermSearch([NotNull] IndexSearcher searcher, [NotNull] string field, [NotNull] string searchTerm)
        {
            Debug.ArgumentNotNull(searcher, nameof(searcher));
            Debug.ArgumentNotNull(field, nameof(field));
            Debug.ArgumentNotNull(searchTerm, nameof(searchTerm));

            var term = new Term(field, searchTerm);
            var query = new TermQuery(term);

            return searcher.Search(query, int.MaxValue);
        }

        [CanBeNull]
        private TopDocs WildCardSearch([NotNull] IndexSearcher searcher, [NotNull] string field, [NotNull] string searchTerm)
        {
            Debug.ArgumentNotNull(searcher, nameof(searcher));
            Debug.ArgumentNotNull(field, nameof(field));
            Debug.ArgumentNotNull(searchTerm, nameof(searchTerm));

            var term = new Term(field, searchTerm);
            var query = new WildcardQuery(term);

            return searcher.Search(query, int.MaxValue);
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

        private void WriteDocument([NotNull] XmlTextWriter output, [NotNull] Document document)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(document, nameof(document));

            var fields = document.GetFields();

            foreach (var f in fields)
            {
                var field = f as Field;
                if (field == null)
                {
                    continue;
                }

                output.WriteStartElement("field");
                output.WriteAttributeString("name", field.Name);
                output.WriteCData(field.StringValue);
                output.WriteEndElement();
            }
        }

        private void WriteDocument([NotNull] XmlTextWriter output, [NotNull] List<string> columns, [NotNull] Document doc)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(columns, nameof(columns));
            Assert.ArgumentNotNull(doc, nameof(doc));

            output.WriteStartElement("document");

            var names = new List<string>();

            foreach (var f in doc.GetFields())
            {
                var field = f as Field;
                if (field == null)
                {
                    continue;
                }

                var name = field.Name;

                var index = 0;
                while (names.Contains(name))
                {
                    name = field.Name + index;
                    index++;
                }

                names.Add(name);

                if (!columns.Contains(name))
                {
                    columns.Add(name);
                }

                output.WriteStartElement("field");
                output.WriteAttributeString("name", name);
                output.WriteCData(field.StringValue);
                output.WriteEndElement();
            }

            output.WriteEndElement();
        }

        private void WriteDocuments([NotNull] XmlTextWriter output, IndexSearcher searcher, [NotNull] TopDocs hits, int offset)
        {
            Debug.ArgumentNotNull(hits, nameof(hits));
            Debug.ArgumentNotNull(output, nameof(output));

            var documents = new List<Document>();

            for (var i = offset; i < offset + 100; i++)
            {
                if (i >= hits.TotalHits)
                {
                    break;
                }

                var d = searcher.Doc(hits.ScoreDocs[i].Doc);

                documents.Add(d);
            }

            WriteDocuments(output, documents, hits.TotalHits);
        }

        private void WriteDocuments([NotNull] XmlTextWriter output, [NotNull] IEnumerable<Document> documents, int total)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(documents, nameof(documents));

            var columns = new List<string>();

            output.WriteStartElement("documents");
            output.WriteAttributeString("count", documents.Count().ToString());
            output.WriteAttributeString("total", total.ToString());

            foreach (var document in documents)
            {
                WriteDocument(output, columns, document);
            }

            output.WriteEndElement();

            WriteColumns(output, columns);
        }

        private void WriteFields([NotNull] XmlTextWriter output, [NotNull] Index index)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(index, nameof(index));

            var reader = GetReader(index);
            if (reader == null)
            {
                return;
            }

            var counts = new Dictionary<string, int>();

            var termEnumerator = reader.Terms();
            while (termEnumerator.Next())
            {
                var term = termEnumerator.Term;
                var field = term.Field;

                int count;
                if (!counts.TryGetValue(field, out count))
                {
                    counts[field] = 1;
                }
                else
                {
                    counts[field] += 1;
                }
            }

            try
            {
                var fieldNames = reader.GetFieldNames(IndexReader.FieldOption.ALL);

                foreach (var fieldName in fieldNames)
                {
                    var name = fieldName;

                    int count;
                    if (!counts.TryGetValue(name, out count))
                    {
                        count = 0;
                    }

                    output.WriteStartElement("field");
                    output.WriteAttributeString("name", name);
                    output.WriteAttributeString("count", count.ToString());
                    output.WriteEndElement();
                }
            }
            finally
            {
                reader.Dispose();
            }
        }

        private void WriteIndexes([NotNull] XmlTextWriter output)
        {
            Debug.ArgumentNotNull(output, nameof(output));

            if (SearchManager.IndexCount <= 0)
            {
                return;
            }

            var configuration = GetConfiguration();
            if (configuration == null || configuration.Indexes.Count <= 0)
            {
                return;
            }

            foreach (var key in configuration.Indexes.Keys)
            {
                var index = configuration.Indexes[key];

                output.WriteStartElement("index");

                output.WriteAttributeString("name", index.Name);
                output.WriteAttributeString("count", index.GetDocumentCount().ToString());

                output.WriteStartElement("fields");
                WriteFields(output, index);
                output.WriteEndElement();

                output.WriteEndElement();
            }
        }
    }
}
