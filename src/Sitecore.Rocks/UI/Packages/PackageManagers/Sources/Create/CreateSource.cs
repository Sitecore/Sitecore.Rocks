// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Packages.PackageManagers.Sources.Create
{
    [PackageSource("Create", "New Package", 9500)]
    public class CreateSource : IPackageSource
    {
        [CanBeNull]
        protected FrameworkElement Viewer { get; set; }

        public void ClearControl()
        {
            Viewer = null;
        }

        public FrameworkElement GetControl(Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            var result = Viewer;
            if (result == null)
            {
                result = new CreatePackageViewer(site);
                Viewer = result;
            }

            return result;
        }
    }
}
