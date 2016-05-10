// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Data;
using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.QueryAnalyzers
{
    public class QueryExporter
    {
        public void ExportTables([NotNull] XmlTextWriter output, [NotNull] List<QueryAnalyzer.ResultDataTable> dataTables, [NotNull] string title)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(dataTables, nameof(dataTables));
            Assert.ArgumentNotNull(title, nameof(title));

            output.WriteStartElement(@"tables");
            if (!string.IsNullOrEmpty(title))
            {
                output.WriteAttributeString("title", title);
            }

            foreach (var dataTable in dataTables)
            {
                output.WriteStartElement(@"table");

                output.WriteStartElement(@"columns");
                ExportColumns(output, dataTable);
                output.WriteEndElement();

                output.WriteStartElement(@"rows");
                ExportRows(output, dataTable);
                output.WriteEndElement();

                output.WriteEndElement();
            }

            output.WriteEndElement();
        }

        private void ExportColumns([NotNull] XmlTextWriter output, [NotNull] QueryAnalyzer.ResultDataTable dataTable)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(dataTable, nameof(dataTable));

            foreach (var column in dataTable.Columns)
            {
                var dataColumn = (DataColumn)column;

                output.WriteStartElement(@"column");
                output.WriteAttributeString(@"name", dataColumn.ColumnName);
                output.WriteEndElement();
            }
        }

        private void ExportRows([NotNull] XmlTextWriter output, [NotNull] QueryAnalyzer.ResultDataTable dataTable)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(dataTable, nameof(dataTable));

            foreach (QueryAnalyzer.ResultDataRow dataRow in dataTable.Rows)
            {
                output.WriteStartElement(@"row");

                if (dataRow.ItemUri != ItemUri.Empty)
                {
                    output.WriteAttributeString(@"id", dataRow.ItemUri.ItemId.ToString());
                    output.WriteAttributeString(@"language", dataRow.Language);
                }

                for (var index = 0; index < dataRow.ItemArray.Length; index++)
                {
                    output.WriteStartElement(@"value");

                    var fieldUri = dataRow.FieldArray[index];
                    if (fieldUri != null)
                    {
                        output.WriteAttributeString(@"id", fieldUri.FieldId.ToString());
                    }

                    var value = dataRow.ItemArray[index];
                    if (value != null)
                    {
                        output.WriteValue(value.ToString());
                    }

                    output.WriteEndElement();
                }

                output.WriteEndElement();
            }
        }
    }
}
