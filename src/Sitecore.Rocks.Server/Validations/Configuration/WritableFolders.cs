// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Validations.Configuration
{
    [Validation("Folder without required write permission", "Configuration")]
    public class WritableFolders : Validation
    {
        public override bool CanCheck(string contextName)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            CheckWritableWebFolder(output, "/upload");
            CheckWritableWebFolder(output, "/temp");
            CheckWritableWebFolder(output, "/sitecore/shell/Applications/debug");
            CheckWritableWebFolder(output, "/sitecore/shell/Controls/debug");
            CheckWritableWebFolder(output, "/layouts");
            CheckWritableWebFolder(output, "/xsl");
            CheckWritableWebFolder(output, "/App_Data");
            CheckWritableWebFolder(output, "/App_Data/MediaCache");
            CheckWritableWebFolder(output, "/indexes");

            CheckWritableDataFolder(output, "/audit");
            CheckWritableDataFolder(output, "/logs");
            CheckWritableDataFolder(output, "/viewstate");
            CheckWritableDataFolder(output, "/diagnostics");
        }

        private void CheckWritableDataFolder([NotNull] ValidationWriter output, [NotNull] string folder)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(folder, nameof(folder));

            folder = Path.Combine(FileUtil.MapPath(Settings.DataFolder), folder);

            CheckWriteableFolder(output, folder);
        }

        private void CheckWritableWebFolder([NotNull] ValidationWriter output, [NotNull] string folder)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(folder, nameof(folder));

            folder = FileUtil.MapPath(folder);

            CheckWriteableFolder(output, folder);
        }

        private void CheckWriteableFolder([NotNull] ValidationWriter output, [NotNull] string folder)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(folder, nameof(folder));

            if (!Directory.Exists(folder))
            {
                return;
            }

            var fileName = Path.Combine(folder, "file.tmp");
            var n = 0;
            while (File.Exists(fileName))
            {
                fileName = Path.Combine(folder, string.Format("file{0}.tmp", n));
                n++;
            }

            try
            {
                FileUtil.WriteToFile(fileName, "write test");
                FileUtil.Delete(fileName);
            }
            catch (Exception ex)
            {
                output.Write(SeverityLevel.Error, "Folder without required write permission", string.Format("The folder \"{0}\" is not writable by the ASP.NET user: {1}", FileUtil.UnmapPath(folder, false), ex.Message), string.Format("Ensure that the ASP.NET user has write permission to the folder: {0}", FileUtil.UnmapPath(folder, false)));
            }
        }
    }
}
