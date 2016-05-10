// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Shell.Environment;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Extensions
{
    public static class FilesExtensions
    {
        [NotNull]
        public static string GetRenderingFileName([NotNull] this FilesHost filesHost, [NotNull] RenderingItem renderingItem, [NotNull] string fileName, [CanBeNull] string extension = null)
        {
            Assert.ArgumentNotNull(filesHost, nameof(filesHost));
            Assert.ArgumentNotNull(renderingItem, nameof(renderingItem));
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            if (string.IsNullOrEmpty(renderingItem.ItemUri.Site.WebRootPath))
            {
                return string.Empty;
            }

            var filePath = IO.File.Normalize(renderingItem.FilePath);
            if (filePath.StartsWith("\\"))
            {
                filePath = filePath.Mid(1);
            }

            fileName = Path.Combine(renderingItem.ItemUri.Site.WebRootPath, filePath);

            if (extension != null)
            {
                fileName = Path.ChangeExtension(fileName, extension);
            }

            return File.Exists(fileName) ? fileName : string.Empty;
        }
    }
}
