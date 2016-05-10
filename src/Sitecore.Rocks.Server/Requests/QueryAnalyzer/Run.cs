// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers;

namespace Sitecore.Rocks.Server.Requests.QueryAnalyzer
{
    public class Run
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string itemId, [NotNull] string script, [NotNull] string max)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(itemId, nameof(itemId));
            Assert.ArgumentNotNull(script, nameof(script));
            Assert.ArgumentNotNull(max, nameof(max));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var item = database.GetRootItem();
            if (!string.IsNullOrEmpty(itemId))
            {
                item = database.GetItem(itemId);
            }

            int maxItems;
            if (!int.TryParse(max, out maxItems))
            {
                maxItems = 0;
            }

            if (maxItems == 0)
            {
                maxItems = int.MaxValue;
            }

            var obj = QueryAnalyzers.QueryAnalyzer.Evaluate(script, item, maxItems);
            if (obj == null)
            {
                return string.Empty;
            }

            var tables = obj as List<object>;
            if (tables == null)
            {
                return string.Empty;
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("tables");

            FormatTables(output, tables);

            output.WriteEndElement();

            return writer.ToString();
        }

        private void FormatTable([NotNull] XmlTextWriter output, [NotNull] object result)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(result, nameof(result));

            var resultSet = result as SelectDataTable;
            if (resultSet != null)
            {
                FormatTable(output, resultSet);
                return;
            }

            FormatValue(output, result);
        }

        private void FormatTable([NotNull] XmlTextWriter output, [NotNull] SelectDataTable selectDataTable)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(selectDataTable, nameof(selectDataTable));

            output.WriteStartElement("columns");

            foreach (var column in selectDataTable.Columns)
            {
                output.WriteStartElement("column");
                output.WriteAttributeString("name", column.Header);
                output.WriteAttributeString("isreadonly", column.IsReadOnly ? "true" : "false");
                output.WriteEndElement();
            }

            output.WriteEndElement();

            output.WriteStartElement("rows");

            foreach (var item in selectDataTable.Items)
            {
                output.WriteStartElement("row");
                output.WriteAttributeString("id", item.Item.ID.ToString());
                output.WriteAttributeString("language", item.Item.Language.Name);
                output.WriteAttributeString("name", item.Item.Name);
                output.WriteAttributeString("path", item.Item.Paths.Path);

                foreach (var field in item.Fields)
                {
                    output.WriteStartElement("value");
                    output.WriteAttributeString("name", field.ColumnName);

                    if (field.Field != null)
                    {
                        output.WriteAttributeString("id", field.Field.ID.ToString());
                    }

                    output.WriteValue(field.Value);
                    output.WriteEndElement();
                }

                output.WriteEndElement();
            }

            output.WriteEndElement();
        }

        private void FormatTables([NotNull] XmlTextWriter output, [NotNull] List<object> tables)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(tables, nameof(tables));

            var count = 0;

            foreach (var table in tables)
            {
                output.WriteStartElement("table");
                output.WriteAttributeString("index", count.ToString());

                FormatTable(output, table);

                output.WriteEndElement();

                count++;
            }
        }

        private void FormatValue([NotNull] XmlTextWriter output, [NotNull] object result)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(result, nameof(result));

            output.WriteStartElement("value");

            output.WriteValue(result.ToString());

            output.WriteEndElement();
        }
    }
}
