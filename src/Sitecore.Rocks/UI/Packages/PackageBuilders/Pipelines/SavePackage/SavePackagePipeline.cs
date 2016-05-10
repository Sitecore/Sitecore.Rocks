// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.UI.Packages.PackageBuilders.Pipelines.SavePackage
{
    public class SavePackagePipeline : Pipeline<SavePackagePipeline>
    {
        [NotNull]
        public XmlTextWriter Output { get; private set; }

        [NotNull]
        public PackageBuilder PackageBuilder { get; private set; }

        [NotNull]
        public SavePackagePipeline WithParameters([NotNull] PackageBuilder packageBuilder, [NotNull] XmlTextWriter output)
        {
            Assert.ArgumentNotNull(packageBuilder, nameof(packageBuilder));
            Assert.ArgumentNotNull(output, nameof(output));

            PackageBuilder = packageBuilder;
            Output = output;

            return Start();
        }
    }
}
