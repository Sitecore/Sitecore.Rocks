// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.SystemTab.Maintenance.Archives
{
    [StartPageControl(StartPageMaintenancePage.Name, 1000)]
    public class StartPageArchivesGroup : StartPageGroupBase
    {
        public const string Name = StartPageMaintenancePage.Name + ".Archives";

        public StartPageArchivesGroup([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Recycle Bin and Archives";
            Description = "Management the archives.";
        }
    }
}
