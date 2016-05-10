// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.DevelopingTab.Deployment.Packages
{
    [StartPageControl(StartPageDeploymentPage.Name, 1000)]
    public class StartPagePackagesGroup : StartPageGroupBase
    {
        public const string Name = StartPageDeploymentPage.Name + ".Packages";

        public StartPagePackagesGroup([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Packages";
            Description = "Packages are zip files containing items and files. Packages are light-weight and usually used for simple transport of content.";
        }
    }
}
