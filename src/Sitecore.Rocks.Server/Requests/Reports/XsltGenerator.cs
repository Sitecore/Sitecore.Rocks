// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Text;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Rocks.Server.Requests.QueryAnalyzer;
using Sitecore.Xml;

namespace Sitecore.Rocks.Server.Requests.Reports
{
    public class XsltGenerator
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string batchScript, [NotNull] string xslt)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(batchScript, nameof(batchScript));
            Assert.ArgumentNotNull(xslt, nameof(xslt));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var doc = GetXmlDocument(databaseName, batchScript);

            var fileName = TempFolder.GetFilename("Report.xslt");
            var xslFile = FileUtil.MapPath(fileName);

            File.WriteAllText(xslFile, xslt, Encoding.UTF8);

            var result = XmlUtil.Transform(doc, xslFile, true);

            File.Delete(xslFile);

            return result;
        }

        [NotNull]
        private XmlDocument GetXmlDocument([NotNull] string databaseName, [NotNull] string batchScript)
        {
            Debug.ArgumentNotNull(databaseName, nameof(databaseName));
            Debug.ArgumentNotNull(batchScript, nameof(batchScript));

            var script = new Run();
            var xml = script.Execute(databaseName, string.Empty, batchScript, "0");

            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc;
        }
    }
}
