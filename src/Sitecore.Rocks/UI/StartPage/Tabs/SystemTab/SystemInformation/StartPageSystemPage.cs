// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.SystemTab.SystemInformation
{
    [StartPageControl(StartPageSystemMainTab.Name, 1000)]
    public class StartPageSystemPage : StartPageListBase
    {
        public const string Name = StartPageSystemMainTab.Name + ".System";

        public StartPageSystemPage([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "System";
        }
    }
}
