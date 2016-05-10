// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Extensions.StringExtensions;
using Sitecore.Install.Framework;
using Sitecore.Install.Zip;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Requests.Packages
{
    public class GetPackageFolder
    {
        [NotNull]
        public string Execute()
        {
            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented
            };

            output.WriteStartElement("packages");

            var folder = FileUtil.MapPath(Settings.PackagePath);

            WritePackages(output, folder);

            output.WriteEndElement();

            return writer.ToString();
        }

        private void WritePackage(XmlTextWriter output, string fileName)
        {
            var folder = FileUtil.MapPath(Settings.PackagePath);
            var localFileName = "package:" + fileName.Mid(folder.Length + 1).Replace("\\", "/");

            var packageAnalyzer = new PackageAnalyzer(new SimpleProcessingContext());

            var reader = new PackageReader(fileName);
            reader.Populate(packageAnalyzer);

            output.WriteStartElement("package");

            output.WriteElementString("filename", localFileName);
            output.WriteElementString("name", packageAnalyzer.Name);
            output.WriteElementString("author", packageAnalyzer.Author);
            output.WriteElementString("version", packageAnalyzer.Version);
            output.WriteElementString("publisher", packageAnalyzer.Publisher);
            output.WriteElementString("license", packageAnalyzer.License);
            output.WriteElementString("comment", packageAnalyzer.Comment);
            output.WriteElementString("readme", packageAnalyzer.Readme);

            output.WriteEndElement();
        }

        private void WritePackages(XmlTextWriter output, string folder)
        {
            foreach (var fileName in Directory.GetFiles(folder))
            {
                var fileInfo = new FileInfo(fileName);
                if ((fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                if ((fileInfo.Attributes & FileAttributes.System) == FileAttributes.System)
                {
                    continue;
                }

                if (string.Compare(Path.GetExtension(fileName), ".zip", StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    continue;
                }

                try
                {
                    var writer = new StringWriter();
                    var inner = new XmlTextWriter(writer)
                    {
                        Formatting = Formatting.Indented
                    };

                    WritePackage(inner, fileName);

                    output.WriteRaw(writer.ToString());
                }
                catch
                {
                    continue;
                }
            }

            foreach (var subfolder in Directory.GetDirectories(folder))
            {
                var fileInfo = new DirectoryInfo(subfolder);
                if ((fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                if ((fileInfo.Attributes & FileAttributes.System) == FileAttributes.System)
                {
                    continue;
                }

                WritePackages(output, subfolder);
            }
        }
    }
}
