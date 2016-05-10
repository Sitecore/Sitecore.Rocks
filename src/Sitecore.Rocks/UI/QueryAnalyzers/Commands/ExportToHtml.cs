// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.QueryAnalyzers.Commands
{
    [Command]
    public class ExportToHtml : CommandBase
    {
        public ExportToHtml()
        {
            Text = Resources.ExportToBrowser_ExportToBrowser_Export_to_Html;
            Group = "Export";
            SortingValue = 2400;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as QueryAnalyzerContext;
            if (context == null)
            {
                return false;
            }

            if (context.QueryAnalyzer.DataGrids.Count == 0)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as QueryAnalyzerContext;
            if (context == null)
            {
                return;
            }

            var reportFileName = GetReportFileName();

            var xml = ExportTables(context.QueryAnalyzer.DataTables);
            var xmlReader = new XmlTextReader(new StringReader(xml));
            var xpathDocument = new XPathDocument(xmlReader);

            var xslt = AppHost.Files.ReadAllText(reportFileName);
            var xsltReader = new XmlTextReader(new StringReader(xslt));

            var transform = new XslCompiledTransform();
            transform.Load(xsltReader);

            var writer = new StringBuilder();
            TextWriter output = new StringWriter(writer);

            transform.Transform(xpathDocument, null, output);

            var result = writer.ToString();

            var tempFileName = Path.ChangeExtension(Path.GetTempFileName(), @".html");

            AppHost.Files.WriteAllText(tempFileName, result, Encoding.UTF8);

            AppHost.Files.Start(tempFileName);
        }

        [NotNull]
        private string ExportTables([NotNull] List<QueryAnalyzer.ResultDataTable> dataTables)
        {
            Debug.ArgumentNotNull(dataTables, nameof(dataTables));

            using (var writer = new StringWriter())
            {
                var output = new XmlTextWriter(writer)
                {
                    Indentation = 2,
                    Formatting = Formatting.Indented,
                    IndentChar = ' '
                };

                var exporter = new QueryExporter();

                exporter.ExportTables(output, dataTables, "Query Analyzer");

                output.Flush();

                writer.Close();

                return writer.ToString();
            }
        }

        [NotNull]
        private string GetReportFileName()
        {
            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            return Path.Combine(folder, @"Report.xslt");
        }
    }
}
