// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Packages.PackageBuilders.Pipelines.LoadPackage
{
    public class LoadPackagePipeline : Pipeline<LoadPackagePipeline>
    {
        [NotNull]
        public PackageBuilder PackageBuilder { get; private set; }

        [NotNull]
        public XElement PackageElement { get; private set; }

        [NotNull]
        public Site Site { get; private set; }

        [NotNull]
        public LoadPackagePipeline WithParameters([NotNull] PackageBuilder packageBuilder, [NotNull] Site site, [NotNull] XElement root)
        {
            Assert.ArgumentNotNull(packageBuilder, nameof(packageBuilder));
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(root, nameof(root));

            PackageBuilder = packageBuilder;
            Site = site;
            PackageElement = root;

            return Start();
        }
    }
}
