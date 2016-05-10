// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Repositories
{
    public static class RepositoryManager
    {
        public const string Folders = "Folders";

        public const string LicenseFiles = "LicenseFiles";

        public const string Packages = "Packages";

        public const string Reports = "Reports";

        public const string SitecoreZip = "SitecoreZip";

        public const string StartPages = "StartPages";

        private static readonly List<string> repositoryNames = new List<string>();

        [NotNull]
        public static IEnumerable<string> RepositoryNames
        {
            get { return repositoryNames; }
        }

        [NotNull]
        public static Repository GetRepository([NotNull] string repositoryName)
        {
            Assert.ArgumentNotNull(repositoryName, nameof(repositoryName));

            return new Repository(repositoryName);
        }

        public static void Register([NotNull] string repositoryName)
        {
            Assert.ArgumentNotNull(repositoryName, nameof(repositoryName));

            repositoryNames.Add(repositoryName);
        }

        public static void Unregister([NotNull] string repositoryName)
        {
            Assert.ArgumentNotNull(repositoryName, nameof(repositoryName));

            repositoryNames.Remove(repositoryName);
        }
    }
}
