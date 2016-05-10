// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Specialized;
using System.IO;
using System.Xml.Linq;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Extensions.XElementExtensions;
using Sitecore.Install.Framework;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Requests.Packages
{
    public class PackagePostStep : IPostStep
    {
        public void Run(ITaskOutput output, NameValueCollection metaData)
        {
            var comment = metaData["Comment"] ?? string.Empty;
            if (string.IsNullOrEmpty(comment))
            {
                return;
            }

            var root = ToXElement(comment);
            if (root == null)
            {
                return;
            }

            var items = root.Element("items");
            if (items != null)
            {
                DeleteItems(items);
            }

            var files = root.Element("files");
            if (files != null)
            {
                DeleteFiles(files);
            }
        }

        private void DeleteFiles(XElement files)
        {
            foreach (var element in files.Elements())
            {
                try
                {
                    var fileName = FileUtil.MapPath(element.GetAttributeValue("filename"));

                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        private void DeleteItems(XElement items)
        {
            foreach (var element in items.Elements())
            {
                try
                {
                    var databaseName = element.GetAttributeValue("database");

                    var database = Factory.GetDatabase(databaseName);
                    if (database == null)
                    {
                        continue;
                    }

                    var item = database.GetItem(element.GetAttributeValue("id"));
                    if (item == null)
                    {
                        continue;
                    }

                    item.Delete();
                }
                catch
                {
                    continue;
                }
            }
        }

        [CanBeNull]
        private XElement ToXElement([NotNull] string text)
        {
            Assert.ArgumentNotNull(text, nameof(text));

            XDocument doc;
            try
            {
                doc = XDocument.Parse(text);
            }
            catch
            {
                return null;
            }

            return doc.Root;
        }
    }
}
