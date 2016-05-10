// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Xml;
using Sitecore.Diagnostics;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Requests.Validations
{
    public class GetValidationReport
    {
        [NotNull]
        public string Execute([NotNull] string contextName)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("validations");

            TempFolder.EnsureFolder();
            var fileName = Path.Combine(FileUtil.MapPath(TempFolder.Folder), contextName + "_validation.xml");
            var tempFileName = fileName + ".tmp";

            if (FileUtil.Exists(tempFileName))
            {
                output.WriteAttributeString("generating", "true");
            }

            if (FileUtil.Exists(fileName))
            {
                var text = FileUtil.ReadUTF8File(fileName);

                output.WriteCData(text);
            }

            output.WriteEndElement();
            output.Flush();

            return writer.ToString();
        }
    }
}
