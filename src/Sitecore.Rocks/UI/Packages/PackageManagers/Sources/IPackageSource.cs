// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Packages.PackageManagers.Sources
{
    public interface IPackageSource
    {
        void ClearControl();

        [NotNull]
        FrameworkElement GetControl([NotNull] Site site);
    }
}
