// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Xml.Linq;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.Packages.PackageManagers.Sources.SitePackages
{
    [PackageSource("<Site>", "Anti Packages", 1100)]
    public class SiteAntiPackagesSource : SitePackagesSource
    {
        protected override bool CanShow(XElement element)
        {
            Debug.ArgumentNotNull(element, nameof(element));

            return element.GetElementValue("name").IndexOf(@"Anti Package", StringComparison.InvariantCultureIgnoreCase) >= 0;
        }
    }
}
