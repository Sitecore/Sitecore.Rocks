// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.SystemTab.SystemInformation.Performance
{
    [StartPageControl(StartPageSystemPage.Name, 1000)]
    public class StartPagePerformanceGroup : StartPageGroupBase
    {
        public const string Name = StartPageSystemPage.Name + ".Performance";

        public StartPagePerformanceGroup([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Performance";
            Description = "Troubleshoot performance issues.";
        }
    }
}
