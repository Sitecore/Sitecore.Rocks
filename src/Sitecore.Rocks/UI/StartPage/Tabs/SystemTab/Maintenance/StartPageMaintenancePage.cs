// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.SystemTab.Maintenance
{
    [StartPageControl(StartPageSystemMainTab.Name, 2000)]
    public class StartPageMaintenancePage : StartPageListBase
    {
        public const string Name = StartPageSystemMainTab.Name + ".Maintenance";

        public StartPageMaintenancePage([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Maintenance";
        }
    }
}
