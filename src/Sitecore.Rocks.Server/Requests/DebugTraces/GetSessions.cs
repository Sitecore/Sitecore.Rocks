// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Xml;
using Sitecore.Extensions.StringExtensions;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Requests.DebugTraces
{
    public class GetSessions
    {
        [NotNull]
        public string Execute()
        {
            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("sessions");

            var folder = FileUtil.MapPath("/temp/diagnostics");

            if (!Directory.Exists(folder))
            {
                return string.Empty;
            }

            foreach (var file in Directory.GetFiles(folder, "trace_*.xml"))
            {
                var info = new FileInfo(file);

                if ((info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                if ((info.Attributes & FileAttributes.System) == FileAttributes.System)
                {
                    continue;
                }

                var name = Path.GetFileNameWithoutExtension(file).Mid(6);

                output.WriteStartElement("session");
                output.WriteAttributeString("name", name);
                output.WriteAttributeString("datetime", DateUtil.ToIsoDate(info.LastWriteTime));

                output.WriteEndElement();
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
