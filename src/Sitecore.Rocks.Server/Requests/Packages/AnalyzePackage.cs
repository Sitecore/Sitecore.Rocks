// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Install.Framework;
using Sitecore.Install.Zip;
using Sitecore.IO;
using Sitecore.Rocks.Server.IO;

namespace Sitecore.Rocks.Server.Requests.Packages
{
    public class AnalyzePackage
    {
        [NotNull]
        public string Execute([NotNull] string fileName)
        {
            var localFileName = LocalFile.MapPath(fileName);

            var packageAnalyzer = new PackageAnalyzer(new SimpleProcessingContext());

            var reader = new PackageReader(localFileName);
            reader.Populate(packageAnalyzer);

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented
            };

            output.WriteStartElement("package");

            output.WriteAttributeString("poststep", packageAnalyzer.PostStep);

            var items = new List<string>();
            foreach (var packageItem in packageAnalyzer.Items)
            {
                var itemKey = packageItem.DatabaseName + packageItem.ID;
                if (items.Contains(itemKey))
                {
                    continue;
                }

                items.Add(itemKey);

                var database = Factory.GetDatabase(packageItem.DatabaseName);
                if (database == null)
                {
                    WritePackageItem(output, packageItem, "Skip: Database not found");
                    continue;
                }

                var item = database.GetItem(packageItem.ID);
                if (item == null)
                {
                    WritePackageItem(output, packageItem, "Add");
                    continue;
                }

                WritePackageItem(output, packageItem, "Overwrite");
            }

            foreach (var packageFile in packageAnalyzer.Files)
            {
                var file = FileUtil.MapPath(packageFile.FileName);

                if (File.Exists(file))
                {
                    WritePackageFile(output, packageFile, "Overwrite");
                }
                else
                {
                    WritePackageFile(output, packageFile, "Add");
                }
            }

            output.WriteEndElement();

            return writer.ToString();
        }

        private static void WritePackageFile(XmlTextWriter output, PackageAnalyzer.PackageFile packageFile, string action)
        {
            output.WriteStartElement("file");

            output.WriteAttributeString("filename", packageFile.FileName);
            output.WriteAttributeString("action", action);

            output.WriteEndElement();
        }

        private static void WritePackageItem(XmlTextWriter output, PackageAnalyzer.PackageItem packageItem, string action)
        {
            output.WriteStartElement("item");

            output.WriteAttributeString("id", packageItem.ID.ToString());
            output.WriteAttributeString("databasename", packageItem.DatabaseName);
            output.WriteAttributeString("name", packageItem.ItemName);
            output.WriteAttributeString("language", packageItem.Language.ToString());
            output.WriteAttributeString("version", packageItem.Version.ToString());
            output.WriteAttributeString("path", packageItem.Path);
            output.WriteAttributeString("action", action);

            output.WriteEndElement();
        }
    }
}
