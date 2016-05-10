// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Packages
{
    public abstract class PackageBuilderBase
    {
        private readonly List<string> files = new List<string>();

        private readonly List<Item> items = new List<Item>();

        protected PackageBuilderBase([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            FileName = fileName;
            PackageName = string.Empty;
            Author = Context.GetUserName();
            Version = string.Empty;
            Publisher = string.Empty;
            License = string.Empty;
            Comment = string.Empty;
            Readme = string.Empty;
            TargetFileFolder = "wwwroot";
            PostStep = string.Empty;
        }

        [NotNull]
        public string Author { get; set; }

        [NotNull]
        public string Comment { get; set; }

        [NotNull]
        public string FileName { get; private set; }

        [NotNull]
        public ICollection<string> Files
        {
            get { return files; }
        }

        [NotNull]
        public ICollection<Item> Items
        {
            get { return items; }
        }

        [NotNull]
        public string License { get; set; }

        [NotNull]
        public string PackageName { get; set; }

        [NotNull]
        public string PostStep { get; set; }

        [NotNull]
        public string Publisher { get; set; }

        [NotNull]
        public string Readme { get; set; }

        [NotNull]
        public string TargetFileFolder { get; set; }

        [NotNull]
        public string Version { get; set; }

        [NotNull]
        public string Build()
        {
            return BuildPackage();
        }

        [NotNull]
        protected abstract string BuildPackage();
    }
}
