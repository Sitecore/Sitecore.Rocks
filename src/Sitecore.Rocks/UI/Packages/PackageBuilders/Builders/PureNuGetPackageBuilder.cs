// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Extensibility.Composition;

namespace Sitecore.Rocks.UI.Packages.PackageBuilders.Builders
{
    [Export(typeof(IPackageBuilder), Priority = 2000)]
    public class PureNuGetPackageBuilder : NuGetPackageBuilderBase
    {
        public PureNuGetPackageBuilder()
        {
            Name = "NuGet Package";
            RequestTypeName = "Packages.BuildPureNuGetPackage";
        }
    }
}
