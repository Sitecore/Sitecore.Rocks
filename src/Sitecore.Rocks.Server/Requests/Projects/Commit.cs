// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Diagnostics;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Requests.Projects
{
    public class Commit
    {
        [NotNull]
        public string Execute([NotNull] string fileName, [NotNull] string fileContents, [NotNull] string siteTimestamp, long siteFileSize, [NotNull] string action, bool isDryRun)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));
            Assert.ArgumentNotNull(fileContents, nameof(fileContents));
            Assert.ArgumentNotNull(siteTimestamp, nameof(siteTimestamp));
            Assert.ArgumentNotNull(action, nameof(action));

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            fileName = FileUtil.NormalizeWebPath(fileName);
            if (!fileName.StartsWith("/"))
            {
                fileName = "/" + fileName;
            }

            fileName = FileUtil.MapPath(fileName);

            if (action.IndexOf("delete", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                DeleteFile(output, fileName, fileContents, siteTimestamp, siteFileSize, action, isDryRun);
                return writer.ToString();
            }

            if (!File.Exists(fileName))
            {
                CreateFile(output, fileName, fileContents, siteTimestamp, action, isDryRun);
                return writer.ToString();
            }

            CommitFile(output, fileName, fileContents, siteTimestamp, siteFileSize, action, isDryRun);

            return writer.ToString();
        }

        private void CommitFile([NotNull] XmlTextWriter result, [NotNull] string fileName, [NotNull] string file, [NotNull] string siteTimestamp, long siteFileSize, [NotNull] string action, bool isDryRun)
        {
            Debug.ArgumentNotNull(result, nameof(result));
            Debug.ArgumentNotNull(fileName, nameof(fileName));
            Debug.ArgumentNotNull(file, nameof(file));
            Debug.ArgumentNotNull(siteTimestamp, nameof(siteTimestamp));
            Debug.ArgumentNotNull(action, nameof(action));

            if (action.IndexOf("overwrite", StringComparison.InvariantCultureIgnoreCase) < 0)
            {
                var info = new FileInfo(fileName);
                long ticks;
                if (!long.TryParse(siteTimestamp, out ticks))
                {
                    ticks = 0;
                }

                if (info.LastWriteTimeUtc.Ticks != ticks || info.Length != siteFileSize)
                {
                    WriteResult(result, fileName, "conflict");
                    return;
                }
            }

            var data = System.Convert.FromBase64String(file);

            if (!isDryRun)
            {
                FileUtil.WriteToFile(fileName, ref data);
            }

            WriteResult(result, fileName, "ok");
        }

        private void CreateFile([NotNull] XmlTextWriter output, [NotNull] string fileName, [NotNull] string file, [NotNull] string siteTimestamp, [NotNull] string action, bool isDryRun)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(fileName, nameof(fileName));
            Debug.ArgumentNotNull(file, nameof(file));
            Debug.ArgumentNotNull(siteTimestamp, nameof(siteTimestamp));
            Debug.ArgumentNotNull(action, nameof(action));

            var data = System.Convert.FromBase64String(file);

            if (isDryRun)
            {
                output.WriteStartElement("commit");
                output.WriteAttributeString("timestamp", siteTimestamp);
                output.WriteAttributeString("filesize", data.Length.ToString());
                output.WriteValue("ok");
                output.WriteEndElement();
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(fileName) ?? string.Empty);
            FileUtil.WriteToFile(fileName, ref data);

            WriteResult(output, fileName, "ok");
        }

        private void DeleteFile([NotNull] XmlTextWriter output, [NotNull] string fileName, [NotNull] string file, [NotNull] string siteTimestamp, long siteFileSize, [NotNull] string action, bool isDryRun)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(fileName, nameof(fileName));
            Debug.ArgumentNotNull(file, nameof(file));
            Debug.ArgumentNotNull(siteTimestamp, nameof(siteTimestamp));
            Debug.ArgumentNotNull(action, nameof(action));

            if (!File.Exists(fileName))
            {
                output.WriteStartElement("commit");
                output.WriteAttributeString("timestamp", string.Empty);
                output.WriteAttributeString("filesize", 0.ToString());
                output.WriteValue("deleted");
                output.WriteEndElement();
                return;
            }

            if (action.IndexOf("overwrite", StringComparison.InvariantCultureIgnoreCase) < 0)
            {
                var info = new FileInfo(fileName);
                long ticks;
                if (!long.TryParse(siteTimestamp, out ticks))
                {
                    ticks = 0;
                }

                if (info.LastWriteTimeUtc.Ticks != ticks || info.Length != siteFileSize)
                {
                    WriteResult(output, fileName, "conflict");
                    return;
                }
            }

            if (!isDryRun)
            {
                FileUtil.Delete(fileName);
            }

            output.WriteStartElement("commit");
            output.WriteAttributeString("timestamp", string.Empty);
            output.WriteAttributeString("filesize", 0.ToString());
            output.WriteValue("deleted");
            output.WriteEndElement();
        }

        private void WriteResult([NotNull] XmlTextWriter output, [NotNull] string fileName, [NotNull] string status)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(fileName, nameof(fileName));
            Debug.ArgumentNotNull(status, nameof(status));

            var info = new FileInfo(fileName);

            output.WriteStartElement("commit");
            output.WriteAttributeString("timestamp", info.LastWriteTimeUtc.Ticks.ToString());
            output.WriteAttributeString("filesize", info.Length.ToString());
            output.WriteValue(status);
            output.WriteEndElement();
        }
    }
}
