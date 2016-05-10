// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Media
{
    public static class MediaManager
    {
        public static void Attach([NotNull] ItemUri itemUri, [NotNull] string sitecorePath, [NotNull] string fileName, [NotNull] GetValueCompleted<bool> uploadCompleted)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(sitecorePath, nameof(sitecorePath));
            Assert.ArgumentNotNull(fileName, nameof(fileName));
            Assert.ArgumentNotNull(uploadCompleted, nameof(uploadCompleted));

            if (!DataService.CheckFileSize(fileName, itemUri.Site.Connection))
            {
                return;
            }

            var data = File.ReadAllBytes(fileName);
            var filePath = sitecorePath + @"/" + Path.GetFileName(fileName);

            itemUri.Site.DataService.Attach(itemUri, filePath, data, uploadCompleted);
        }

        public static void Detach([NotNull] ItemUri itemUri, [NotNull] GetValueCompleted<bool> detachCompleted)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(detachCompleted, nameof(detachCompleted));

            itemUri.Site.DataService.Detach(itemUri, detachCompleted);
        }

        public static void DownloadAttachment([NotNull] ItemUri itemUri, [NotNull] string fileName)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            itemUri.Site.DataService.DownloadAttachment(itemUri, value => File.WriteAllBytes(fileName, value));
        }

        public static string GetMediaUrl([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            var path = site.GetHost();

            if (site.SitecoreVersion >= Constants.Versions.Version80)
            {
                path += "/sitecore/shell";
            }

            path += "/~/media/";

            return path;
        }

        public static void Upload([NotNull] DatabaseUri databaseUri, [NotNull] string parentPath, [NotNull] string[] fileNames, [NotNull] GetValueCompleted<ItemHeader> uploadCompleted)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(parentPath, nameof(parentPath));
            Assert.ArgumentNotNull(fileNames, nameof(fileNames));
            Assert.ArgumentNotNull(uploadCompleted, nameof(uploadCompleted));

            foreach (var fileName in fileNames)
            {
                Upload(databaseUri, parentPath, fileName, uploadCompleted);
            }
        }

        public static void Upload([NotNull] DatabaseUri databaseUri, [NotNull] string parentPath, [NotNull] string fileName, [NotNull] GetValueCompleted<ItemHeader> uploadCompleted)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(parentPath, nameof(parentPath));
            Assert.ArgumentNotNull(fileName, nameof(fileName));
            Assert.ArgumentNotNull(uploadCompleted, nameof(uploadCompleted));

            if (!DataService.CheckFileSize(fileName, databaseUri.Site.Connection))
            {
                return;
            }

            var data = File.ReadAllBytes(fileName);
            var itemPath = parentPath + @"/" + Path.GetFileName(fileName);

            GetValueCompleted<ItemHeader> c = delegate(ItemHeader itemHeader)
            {
                Notifications.RaiseMediaUploaded(databaseUri.Site, itemHeader);

                uploadCompleted(itemHeader);
            };

            databaseUri.Site.DataService.Upload(databaseUri, itemPath, data, c);
        }
    }
}
