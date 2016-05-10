// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Commands.Commands
{
    public abstract class ReportCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            var reportFileName = GetReportFileName();

            var xml = GenerateReport(parameter);

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

            AppHost.Browsers.Navigate(tempFileName);
        }

        protected abstract void GenerateReport([NotNull] XmlTextWriter output, [CanBeNull] object parameter);

        protected void WriteColumn([NotNull] XmlTextWriter output, [NotNull] string name)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(name, nameof(name));

            output.WriteStartElement(@"column");
            output.WriteAttributeString(@"name", name);
            output.WriteEndElement();
        }

        protected void WriteColumns([NotNull] XmlTextWriter output, [NotNull] params string[] columns)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(columns, nameof(columns));

            output.WriteStartElement(@"columns");

            foreach (var column in columns)
            {
                WriteColumn(output, column);
            }

            output.WriteEndElement();
        }

        protected void WriteEndTable([NotNull] XmlTextWriter output)
        {
            Debug.ArgumentNotNull(output, nameof(output));

            output.WriteEndElement();
            output.WriteEndElement();
            output.WriteEndElement();
        }

        protected void WriteRow([NotNull] XmlTextWriter output, [NotNull] params string[] values)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(values, nameof(values));

            output.WriteStartElement(@"row");

            foreach (var value in values)
            {
                WriteValue(output, value);
            }

            output.WriteEndElement();
        }

        protected void WriteStartTable([NotNull] XmlTextWriter output, [NotNull] string title, [NotNull] params string[] columns)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(title, nameof(title));
            Debug.ArgumentNotNull(columns, nameof(columns));

            output.WriteStartElement(@"tables");
            output.WriteAttributeString("title", title);

            output.WriteStartElement(@"table");

            WriteColumns(output, columns);

            output.WriteStartElement(@"rows");
        }

        protected void WriteValue([NotNull] XmlTextWriter output, [NotNull] string value)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(value, nameof(value));

            output.WriteStartElement(@"value");
            output.WriteValue(value);
            output.WriteEndElement();
        }

        [NotNull]
        private string GenerateReport([CanBeNull] object parameter)
        {
            using (var writer = new StringWriter())
            {
                var output = new XmlTextWriter(writer)
                {
                    Indentation = 2,
                    Formatting = Formatting.Indented,
                    IndentChar = ' '
                };

                GenerateReport(output, parameter);

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
