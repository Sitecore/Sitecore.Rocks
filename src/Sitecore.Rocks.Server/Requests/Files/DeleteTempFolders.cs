// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Requests.Files
{
    public class DeleteTempFolders
    {
        [NotNull]
        public string Execute([NotNull] string folders, [NotNull] string date)
        {
            Assert.ArgumentNotNull(folders, nameof(folders));
            Assert.ArgumentNotNull(date, nameof(date));

            var timestamp = DateUtil.IsoDateToDateTime(date);

            foreach (var folder in folders.Split('|'))
            {
                switch (folder)
                {
                    case "temp":
                        Delete(FileUtil.MapPath(TempFolder.Folder), timestamp);
                        break;
                    case "logs":
                        Delete(Path.Combine(FileUtil.MapPath(Settings.DataFolder), "logs"), timestamp);
                        break;
                    case "datadebug":
                        Delete(Path.Combine(FileUtil.MapPath(Settings.DataFolder), "debug"), timestamp);
                        break;
                    case "diagnostics":
                        Delete(Path.Combine(FileUtil.MapPath(Settings.DataFolder), "diagnostics"), timestamp);
                        break;
                    case "packages":
                        Delete(Path.Combine(FileUtil.MapPath(Settings.DataFolder), "packages"), timestamp);
                        break;
                    case "viewstate":
                        Delete(Path.Combine(FileUtil.MapPath(Settings.DataFolder), "viewstate"), timestamp);
                        break;
                    case "mediacache":
                        Delete(FileUtil.MapPath("/App_Data/MediaCache"), timestamp);
                        break;
                    case "applicationdebug":
                        Delete(FileUtil.MapPath("/sitecore/shell/Applications/debug"), timestamp);
                        break;
                    case "controldebug":
                        Delete(FileUtil.MapPath("/sitecore/shell/Controls/debug"), timestamp);
                        break;
                }
            }

            return string.Empty;
        }

        private int Delete([NotNull] string folder, DateTime timestamp)
        {
            Debug.ArgumentNotNull(folder, nameof(folder));

            var result = 0;

            foreach (var subfolder in Directory.GetDirectories(folder))
            {
                try
                {
                    result += Delete(subfolder, timestamp);

                    if (result == 0)
                    {
                        Directory.Delete(subfolder);
                    }
                }
                catch
                {
                    continue;
                }
            }

            foreach (var file in Directory.GetFiles(folder))
            {
                if (File.GetLastAccessTime(file) > timestamp)
                {
                    result++;
                    continue;
                }

                File.Delete(file);
            }

            return result;
        }
    }
}
