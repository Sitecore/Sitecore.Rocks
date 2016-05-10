// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Xml;
using System.Xml.XPath;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Extensions.StringExtensions;
using Sitecore.IO;
using Sitecore.Rocks.Server.Validations;
using Sitecore.Xml;

namespace Sitecore.Rocks.Server.Requests.Validations
{
    public class TestQuery
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string query, int checkType)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(query, nameof(query));

            var type = (CustomValidationType)checkType;

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("results");

            switch (type)
            {
                case CustomValidationType.Query:
                    ProcessQuery(output, databaseName, query);
                    break;
                case CustomValidationType.XPath:
                    ProcessXPath(output, databaseName, query);
                    break;
                case CustomValidationType.WebConfig:
                    ProcessWebConfig(output, query);
                    break;
                case CustomValidationType.ExpandedWebConfig:
                    ProcessExpandedWebConfig(output, query);
                    break;
                case CustomValidationType.WebFileSystem:
                    ProcessFileSystem(output, FileUtil.MapPath("/"), query);
                    break;
                case CustomValidationType.DataFileSystem:
                    ProcessFileSystem(output, FileUtil.MapPath(Settings.DataFolder), query);
                    break;
            }

            output.WriteEndElement();
            output.Flush();

            return writer.ToString();
        }

        private void ProcessExpandedWebConfig([NotNull] XmlTextWriter output, [NotNull] string query)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(query, nameof(query));

            var expandedWebConfig = Factory.GetConfiguration();

            XmlNodeList nodes;
            try
            {
                nodes = expandedWebConfig.SelectNodes(query);
            }
            catch
            {
                return;
            }

            if (nodes == null)
            {
                return;
            }

            foreach (XmlNode node in nodes)
            {
                var path = XmlUtil.GetPath(node);
                output.WriteElementString("result", path);
            }
        }

        private void ProcessFileSystem(XmlTextWriter output, string rootFolder, string query)
        {
            if (rootFolder.EndsWith("\\"))
            {
                rootFolder = rootFolder.Left(rootFolder.Length - 1);
            }

            var webFileSystem = new FileSystemNavigator(rootFolder);
            webFileSystem.MoveToFirstChild();

            XPathNodeIterator nodes;
            try
            {
                nodes = webFileSystem.Select(query);
            }
            catch
            {
                return;
            }

            if (nodes == null)
            {
                return;
            }

            foreach (XPathNavigator navigator in nodes)
            {
                var path = string.Empty;
                var n = navigator.Clone();

                while (n != null)
                {
                    if (!string.IsNullOrEmpty(n.LocalName))
                    {
                        path = "\\" + n.LocalName + path;
                    }

                    if (!n.MoveToParent())
                    {
                        break;
                    }
                }

                output.WriteElementString("result", path);
            }
        }

        private void ProcessQuery([NotNull] XmlTextWriter output, [NotNull] string databaseName, [NotNull] string query)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(databaseName, nameof(databaseName));
            Debug.ArgumentNotNull(query, nameof(query));

            var database = Factory.GetDatabase(databaseName);
            Item[] items;
            try
            {
                items = database.SelectItems(query);
            }
            catch
            {
                return;
            }

            foreach (var item in items)
            {
                output.WriteElementString("result", item.Paths.Path);
            }
        }

        private void ProcessWebConfig([NotNull] XmlTextWriter output, [NotNull] string query)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(query, nameof(query));

            var webConfig = new XmlDocument();
            webConfig.Load(FileUtil.MapPath("/web.config"));

            XmlNodeList nodes;
            try
            {
                nodes = webConfig.SelectNodes(query);
            }
            catch
            {
                return;
            }

            if (nodes == null)
            {
                return;
            }

            foreach (XmlNode node in nodes)
            {
                var path = XmlUtil.GetPath(node);
                output.WriteElementString("result", path);
            }
        }

        private void ProcessXPath([NotNull] XmlTextWriter output, [NotNull] string databaseName, [NotNull] string query)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(databaseName, nameof(databaseName));
            Debug.ArgumentNotNull(query, nameof(query));

            var database = Factory.GetDatabase(databaseName);

            ItemList items;
            try
            {
                items = database.SelectItemsUsingXPath(query);
            }
            catch
            {
                return;
            }

            foreach (var item in items)
            {
                output.WriteElementString("result", item.Paths.Path);
            }
        }
    }
}
