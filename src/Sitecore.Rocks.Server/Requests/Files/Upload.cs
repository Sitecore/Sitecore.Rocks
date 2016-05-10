// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using Sitecore.Diagnostics;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Requests.Files
{
    public class Upload
    {
        [NotNull]
        public string Execute([NotNull] string filePath, [NotNull] string file)
        {
            Assert.ArgumentNotNull(filePath, nameof(filePath));
            Assert.ArgumentNotNull(file, nameof(file));

            using (var source = new MemoryStream(System.Convert.FromBase64String(file)))
            {
                var fileName = FileUtil.MapPath(filePath);

                Directory.CreateDirectory(Path.GetDirectoryName(fileName));

                using (var target = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    FileUtil.CopyStream(source, target);
                }
            }

            return string.Empty;
        }
    }
}
