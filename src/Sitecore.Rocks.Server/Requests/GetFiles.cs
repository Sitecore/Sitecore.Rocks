// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Extensions.StringExtensions;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Requests
{
    public class GetFiles
    {
        private string dataFolder;

        [NotNull]
        public string Execute([NotNull] string folder)
        {
            Assert.ArgumentNotNull(folder, nameof(folder));

            dataFolder = FileUtil.MapPath(Settings.DataFolder).Replace("\\", "/");

            folder = FileUtil.NormalizeWebPath(folder);
            if (folder.EndsWith("/"))
            {
                folder = folder.Left(folder.Length - 1);
            }

            string prefix;
            if (folder.StartsWith("web:"))
            {
                var path = FileUtil.NormalizeWebPath(folder.Mid(4)).TrimStart('/');
                folder = Path.Combine(FileUtil.MapPath("/"), path);
                prefix = "web:";
            }
            else if (folder.StartsWith("data:"))
            {
                var path = FileUtil.NormalizeWebPath(folder.Mid(5)).TrimStart('/');
                folder = Path.Combine(FileUtil.MapPath(Settings.DataFolder), path);
                prefix = "data:";
            }
            else
            {
                var path = FileUtil.NormalizeWebPath(folder).TrimStart('/');
                folder = Path.Combine(FileUtil.MapPath("/"), path);
                prefix = "web:";
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("files");

            WriteFolders(output, folder, prefix);
            WriteFiles(output, folder, prefix);

            output.WriteEndElement();

            return writer.ToString();
        }

        [NotNull]
        private string UnmapPath([NotNull] string fileName, [NotNull] string prefix)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));
            Debug.ArgumentNotNull(prefix, nameof(prefix));

            var path = FileUtil.UnmapPath(fileName, false).Replace("\\", "/");

            // UnmapPath work-around
            if (prefix == "data:")
            {
                if (path.StartsWith("/data/"))
                {
                    path = path.Mid(5);
                }
                else
                {
                    if (path.StartsWith(dataFolder, StringComparison.InvariantCultureIgnoreCase))
                    {
                        path = path.Mid(dataFolder.Length);
                    }
                }
            }

            return prefix + path;
        }

        private void WriteFiles([NotNull] XmlTextWriter output, [NotNull] string folder, [NotNull] string prefix)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(folder, nameof(folder));
            Debug.ArgumentNotNull(prefix, nameof(prefix));

            foreach (var fileName in Directory.GetFiles(folder))
            {
                var fileAttributes = File.GetAttributes(fileName);
                if ((fileAttributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                if ((fileAttributes & FileAttributes.System) == FileAttributes.System)
                {
                    continue;
                }

                var fileInfo = new FileInfo(fileName);

                var path = UnmapPath(fileName, prefix);

                output.WriteStartElement("path");

                output.WriteAttributeString("path", path);
                output.WriteAttributeString("name", Path.GetFileName(fileName));
                output.WriteAttributeString("type", "file");
                output.WriteAttributeString("updated", DateUtil.ToIsoDate(fileInfo.LastWriteTimeUtc));
                output.WriteAttributeString("filename", fileName);

                output.WriteEndElement();
            }
        }

        private void WriteFolders([NotNull] XmlTextWriter output, [NotNull] string folder, [NotNull] string prefix)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(folder, nameof(folder));
            Debug.ArgumentNotNull(prefix, nameof(prefix));

            foreach (var directory in Directory.GetDirectories(folder))
            {
                var fileAttributes = File.GetAttributes(directory);
                if ((fileAttributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                if ((fileAttributes & FileAttributes.System) == FileAttributes.System)
                {
                    continue;
                }

                var path = UnmapPath(directory, prefix);

                var directoryInfo = new DirectoryInfo(directory);

                output.WriteStartElement("path");

                output.WriteAttributeString("path", path);
                output.WriteAttributeString("name", Path.GetFileName(directory));
                output.WriteAttributeString("type", "folder");
                output.WriteAttributeString("updated", DateUtil.ToIsoDate(directoryInfo.LastWriteTimeUtc));
                output.WriteAttributeString("filename", directory);

                output.WriteEndElement();
            }
        }
    }
}
