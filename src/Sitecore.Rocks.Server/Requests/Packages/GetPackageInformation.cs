// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Xml;
using Sitecore.Install.Framework;
using Sitecore.Install.Zip;
using Sitecore.Rocks.Server.IO;

namespace Sitecore.Rocks.Server.Requests.Packages
{
    public class GetPackageInformation
    {
        [NotNull]
        public string Execute([NotNull] string fileName)
        {
            var file = LocalFile.MapPath(fileName);

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented
            };

            WritePackage(output, file);

            return writer.ToString();
        }

        private void WritePackage(XmlTextWriter output, string fileName)
        {
            var packageAnalyzer = new PackageAnalyzer(new SimpleProcessingContext());

            var reader = new PackageReader(fileName);
            reader.Populate(packageAnalyzer);

            output.WriteStartElement("package");

            output.WriteElementString("filename", fileName);
            output.WriteElementString("name", packageAnalyzer.Name);
            output.WriteElementString("author", packageAnalyzer.Author);
            output.WriteElementString("version", packageAnalyzer.Version);
            output.WriteElementString("publisher", packageAnalyzer.Publisher);
            output.WriteElementString("license", packageAnalyzer.License);
            output.WriteElementString("comment", packageAnalyzer.Comment);
            output.WriteElementString("readme", packageAnalyzer.Readme);

            output.WriteEndElement();
        }
    }
}
