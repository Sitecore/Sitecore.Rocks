// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Diagnostics;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Requests.Files
{
    public class Delete
    {
        [NotNull]
        public string Execute([NotNull] string filePath)
        {
            Assert.ArgumentNotNull(filePath, nameof(filePath));

            FileUtil.Delete(filePath);

            return string.Empty;
        }
    }
}
