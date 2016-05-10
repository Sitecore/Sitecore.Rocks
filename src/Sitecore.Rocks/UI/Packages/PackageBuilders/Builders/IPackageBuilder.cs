// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Packages.PackageBuilders.Builders
{
    public interface IPackageBuilder
    {
        [NotNull]
        string Author { get; set; }

        [NotNull]
        string Comment { get; set; }

        [NotNull]
        string FileName { get; set; }

        [NotNull]
        IEnumerable<PackageFile> Files { get; set; }

        [NotNull]
        IEnumerable<PackageItem> Items { get; set; }

        [NotNull]
        string License { get; set; }

        [NotNull]
        string Name { get; }

        [NotNull]
        string PackageName { get; set; }

        [NotNull]
        string Publisher { get; set; }

        [NotNull]
        string Readme { get; set; }

        [NotNull]
        Site Site { get; set; }

        [NotNull]
        string TargetFileFolder { get; set; }

        [NotNull]
        string Version { get; set; }

        void Build([NotNull] Action<string> complected);

        bool IsValid();
    }
}
