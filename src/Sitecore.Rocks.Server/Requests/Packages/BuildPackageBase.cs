// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Rocks.Server.Packages;
using Sitecore.Web;

namespace Sitecore.Rocks.Server.Requests.Packages
{
    public abstract class BuildPackageBase
    {
        [NotNull]
        protected string Build([NotNull] string itemList, [NotNull] string fileList, [NotNull] string packageName, [NotNull] string author, [NotNull] string version, [NotNull] string publisher, [NotNull] string license, [NotNull] string comment, [NotNull] string readme, [NotNull] string targetFileFolder)
        {
            Debug.ArgumentNotNull(itemList, nameof(itemList));
            Debug.ArgumentNotNull(fileList, nameof(fileList));
            Debug.ArgumentNotNull(packageName, nameof(packageName));
            Debug.ArgumentNotNull(author, nameof(author));
            Debug.ArgumentNotNull(version, nameof(version));
            Debug.ArgumentNotNull(publisher, nameof(publisher));
            Debug.ArgumentNotNull(license, nameof(license));
            Debug.ArgumentNotNull(comment, nameof(comment));
            Debug.ArgumentNotNull(readme, nameof(readme));
            Debug.ArgumentNotNull(targetFileFolder, nameof(targetFileFolder));

            var fileName = TempFolder.GetFilename("package.zip");

            var package = GetBuilder(fileName);

            package.PackageName = packageName;
            package.Author = author;
            package.Version = version;
            package.Publisher = publisher;
            package.License = license;
            package.Comment = comment;
            package.Readme = readme;
            package.TargetFileFolder = targetFileFolder;

            foreach (var part in itemList.Split('|'))
            {
                if (string.IsNullOrEmpty(part))
                {
                    continue;
                }

                var tuple = part.Split(',');

                var database = Factory.GetDatabase(tuple[0]);
                if (database == null)
                {
                    continue;
                }

                var item = database.GetItem(tuple[1]);
                if (item != null)
                {
                    package.Items.Add(item);
                }
            }

            foreach (var part in fileList.Split('|'))
            {
                if (string.IsNullOrEmpty(part))
                {
                    continue;
                }

                if (FileUtil.Exists(part) || FileUtil.FolderExists(part))
                {
                    package.Files.Add(part);
                }
            }

            var result = package.Build();

            var unmappedFileName = FileUtil.UnmapPath(result, false);
            if (unmappedFileName.Contains(":"))
            {
                return unmappedFileName;
            }

            return WebUtil.GetServerUrl() + unmappedFileName;
        }

        [NotNull]
        protected abstract PackageBuilderBase GetBuilder([NotNull] string fileName);
    }
}
