// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Requests.Files
{
    public class GetTempFolders
    {
        [NotNull]
        public string Execute()
        {
            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("folders");

            WriteFolder(output, FileUtil.MapPath(TempFolder.Folder), "temp");
            WriteFolder(output, Path.Combine(FileUtil.MapPath(Settings.DataFolder), "logs"), "logs");
            WriteFolder(output, Path.Combine(FileUtil.MapPath(Settings.DataFolder), "debug"), "datadebug");
            WriteFolder(output, Path.Combine(FileUtil.MapPath(Settings.DataFolder), "diagnostics"), "diagnostics");
            WriteFolder(output, Path.Combine(FileUtil.MapPath(Settings.DataFolder), "packages"), "packages");
            WriteFolder(output, Path.Combine(FileUtil.MapPath(Settings.DataFolder), "viewstate"), "viewstate");
            WriteFolder(output, FileUtil.MapPath("/App_Data/MediaCache"), "mediacache");
            WriteFolder(output, FileUtil.MapPath("/sitecore/shell/Applications/debug"), "applicationdebug");
            WriteFolder(output, FileUtil.MapPath("/sitecore/shell/Controls/debug"), "controldebug");

            output.WriteEndElement();

            return writer.ToString();
        }

        private void GetFolderSize([NotNull] string folder, ref long size, ref DateTime lastWrite)
        {
            Debug.ArgumentNotNull(folder, nameof(folder));

            foreach (var file in Directory.GetFiles(folder))
            {
                var info = new FileInfo(file);
                size += info.Length;

                if (info.LastWriteTime > lastWrite)
                {
                    lastWrite = info.LastWriteTime;
                }
            }

            foreach (var subfolder in Directory.GetDirectories(folder))
            {
                GetFolderSize(subfolder, ref size, ref lastWrite);
            }
        }

        private void WriteFolder([NotNull] XmlTextWriter output, [NotNull] string path, [NotNull] string name)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(path, nameof(path));
            Debug.ArgumentNotNull(name, nameof(name));

            output.WriteStartElement("folder");

            output.WriteAttributeString("name", name);
            output.WriteAttributeString("path", FileUtil.UnmapPath(path, false));

            var lastWrite = DateTime.MinValue;
            long size = 0;
            try
            {
                GetFolderSize(path, ref size, ref lastWrite);
            }
            catch
            {
                size = -1;
            }

            output.WriteAttributeString("size", size.ToString());
            output.WriteAttributeString("lastwrite", DateUtil.ToIsoDate(lastWrite));

            output.WriteEndElement();
        }
    }
}
