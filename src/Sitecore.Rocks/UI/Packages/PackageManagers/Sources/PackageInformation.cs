// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Packages.PackageManagers.Sources
{
    public class PackageInformation
    {
        public PackageInformation([NotNull] Site site, [NotNull] string packageName, [NotNull] string author, [NotNull] string version, [NotNull] string publisher, [NotNull] string license, [NotNull] string comment, [NotNull] string readme)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(packageName, nameof(packageName));
            Assert.ArgumentNotNull(author, nameof(author));
            Assert.ArgumentNotNull(version, nameof(version));
            Assert.ArgumentNotNull(publisher, nameof(publisher));
            Assert.ArgumentNotNull(license, nameof(license));
            Assert.ArgumentNotNull(comment, nameof(comment));
            Assert.ArgumentNotNull(readme, nameof(readme));

            Site = site;
            PackageName = packageName;
            Author = author;
            Version = version;
            Publisher = publisher;
            License = license;
            Comment = comment;
            Readme = readme;
        }

        public PackageInformation([NotNull] Site site, [NotNull] PackageAnalyzer packageAnalyzer)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(packageAnalyzer, nameof(packageAnalyzer));

            Site = site;
            LocalFileName = packageAnalyzer.FileName;
            PackageName = packageAnalyzer.Name;
            Author = packageAnalyzer.Author;
            Version = packageAnalyzer.Version;
            Publisher = packageAnalyzer.Publisher;
            License = packageAnalyzer.License;
            Comment = packageAnalyzer.Comment;
            Readme = packageAnalyzer.Readme;

            if (string.IsNullOrEmpty(PackageName))
            {
                PackageName = Path.GetFileNameWithoutExtension(LocalFileName) ?? "[Unknown Package]";
            }
        }

        [NotNull]
        public string Author { get; private set; }

        [NotNull]
        public string Comment { get; private set; }

        [NotNull]
        public string License { get; private set; }

        [NotNull]
        public string LocalFileName { get; set; }

        [NotNull]
        public string PackageName { get; }

        [NotNull]
        public string Publisher { get; private set; }

        [NotNull]
        public string Readme { get; private set; }

        [NotNull]
        public string ServerFileName { get; set; }

        [NotNull]
        public Site Site { get; private set; }

        [NotNull]
        public string Version { get; private set; }
    }
}
