// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Packages
{
    public class PureNuGetPackageBuilder : NuGetPackageBuilderBase
    {
        public PureNuGetPackageBuilder([NotNull] string fileName) : base(fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));
        }
    }
}
