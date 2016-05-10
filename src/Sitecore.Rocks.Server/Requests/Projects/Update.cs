// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Diagnostics;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Requests.Projects
{
    public class Update
    {
        [NotNull]
        public string Execute([NotNull] string fileName, [NotNull] string serverTimestamp, long serverFileSize, [NotNull] string action)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));
            Assert.ArgumentNotNull(serverTimestamp, nameof(serverTimestamp));
            Assert.ArgumentNotNull(action, nameof(action));

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            fileName = FileUtil.NormalizeWebPath(fileName);
            if (!fileName.StartsWith("/"))
            {
                fileName = "/" + fileName;
            }

            fileName = FileUtil.MapPath(fileName);

            if (!File.Exists(fileName))
            {
                output.WriteStartElement("commit");
                output.WriteAttributeString("status", "deleted");
                output.WriteAttributeString("timestamp", string.Empty);
                output.WriteAttributeString("filesize", 0.ToString());
                output.WriteEndElement();

                return writer.ToString();
            }

            if (action.IndexOf("revert", StringComparison.InvariantCultureIgnoreCase) < 0)
            {
                var info = new FileInfo(fileName);
                long ticks;
                if (!long.TryParse(serverTimestamp, out ticks))
                {
                    ticks = 0;
                }

                if (info.LastWriteTimeUtc.Ticks == ticks && info.Length == serverFileSize)
                {
                    WriteResult(output, string.Empty, fileName, "unchanged");
                    return writer.ToString();
                }
            }

            var file = File.ReadAllBytes(fileName);

            var data = System.Convert.ToBase64String(file);

            WriteResult(output, data, fileName, "ok");

            return writer.ToString();
        }

        private void WriteResult([NotNull] XmlTextWriter output, [NotNull] string data, [NotNull] string fileName, [NotNull] string status)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(data, nameof(data));
            Debug.ArgumentNotNull(fileName, nameof(fileName));
            Debug.ArgumentNotNull(status, nameof(status));

            var info = new FileInfo(fileName);

            output.WriteStartElement("commit");
            output.WriteAttributeString("status", status);
            output.WriteAttributeString("timestamp", info.LastWriteTimeUtc.Ticks.ToString());
            output.WriteAttributeString("filesize", info.Length.ToString());

            output.WriteValue(data);

            output.WriteEndElement();
        }
    }
}
