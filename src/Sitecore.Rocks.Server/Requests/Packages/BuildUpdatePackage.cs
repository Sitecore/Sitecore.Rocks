// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Packages;

namespace Sitecore.Rocks.Server.Requests.Packages
{
    public class BuildUpdatePackage : BuildPackageBase
    {
        [NotNull]
        public string Execute([NotNull] string itemList, [NotNull] string fileList, [NotNull] string packageName, [NotNull] string author, [NotNull] string version, [NotNull] string publisher, [NotNull] string license, [NotNull] string comment, [NotNull] string readme)
        {
            Assert.ArgumentNotNull(itemList, nameof(itemList));
            Assert.ArgumentNotNull(fileList, nameof(fileList));
            Assert.ArgumentNotNull(packageName, nameof(packageName));
            Assert.ArgumentNotNull(author, nameof(author));
            Assert.ArgumentNotNull(version, nameof(version));
            Assert.ArgumentNotNull(publisher, nameof(publisher));
            Assert.ArgumentNotNull(license, nameof(license));
            Assert.ArgumentNotNull(comment, nameof(comment));
            Assert.ArgumentNotNull(readme, nameof(readme));

            return Build(itemList, fileList, packageName, author, version, publisher, license, comment, readme, string.Empty);
        }

        protected override PackageBuilderBase GetBuilder(string fileName)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));

            return new UpdatePackageBuilder(fileName);
        }
    }
}
