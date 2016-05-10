// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.DevelopingTab.Deployment
{
    [StartPageControl(StartPageDevelopingMainTab.Name, 3000)]
    public class StartPageDeploymentPage : StartPageListBase
    {
        public const string Name = StartPageDevelopingMainTab.Name + ".Deployment";

        public StartPageDeploymentPage([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Deployment";
        }
    }
}
