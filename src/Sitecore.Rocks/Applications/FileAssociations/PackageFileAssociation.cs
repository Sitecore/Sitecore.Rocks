// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Packages.PackageBuilders;

namespace Sitecore.Rocks.Applications.FileAssociations
{
    [FileAssociation(".package")]
    public class PackageFileAssociation : IFileAssociation
    {
        public void Open(string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            PackageBuilder.Load(fileName, null, null);
        }
    }
}
