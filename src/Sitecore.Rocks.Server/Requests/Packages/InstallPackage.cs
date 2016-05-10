// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Rocks.Server.IO;
using Sitecore.Rocks.Server.Packages;

namespace Sitecore.Rocks.Server.Requests.Packages
{
    public class InstallPackage
    {
        [NotNull]
        public string Execute([NotNull] string file, [NotNull] string uploadedFile)
        {
            Assert.ArgumentNotNull(uploadedFile, nameof(uploadedFile));
            Assert.ArgumentNotNull(file, nameof(file));

            string fileName;

            if (!string.IsNullOrEmpty(uploadedFile))
            {
                fileName = Upload(file);
            }
            else
            {
                fileName = LocalFile.MapPath(file);
            }

            var installer = new PackageInstaller(fileName);

            installer.Install();

            return string.Empty;
        }

        [NotNull]
        private string Upload([NotNull] string uploadedFile)
        {
            Debug.ArgumentNotNull(uploadedFile, nameof(uploadedFile));

            var tempFileName = TempFolder.GetFilename("package.zip");

            var fileName = FileUtil.MapPath(tempFileName);
            using (var source = new MemoryStream(System.Convert.FromBase64String(uploadedFile)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));

                using (var target = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    FileUtil.CopyStream(source, target);
                }
            }

            return FileUtil.UnmapPath(fileName, false);
        }
    }
}
