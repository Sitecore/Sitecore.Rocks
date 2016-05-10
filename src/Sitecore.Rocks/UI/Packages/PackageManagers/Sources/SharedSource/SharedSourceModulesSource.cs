// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Packages.PackageManagers.Sources.SharedSource
{
    [PackageSource("Shared Source", "Modules", 5000)]
    public class SharedSourceModulesSource : IPackageSource
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
                result = new SharedSourceViewer();
                Viewer = result;
            }

            return result;
        }
    }
}
