// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.SystemTab.SystemInformation.SystemHealth
{
    [StartPageControl(StartPageSystemPage.Name, 3000)]
    public class StartPageSystemHealthGroup : StartPageGroupBase
    {
        public const string Name = StartPageSystemPage.Name + ".SystemHealth";

        public StartPageSystemHealthGroup([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "System Health";
            Description = "Find and identify system issues.";
        }
    }
}
