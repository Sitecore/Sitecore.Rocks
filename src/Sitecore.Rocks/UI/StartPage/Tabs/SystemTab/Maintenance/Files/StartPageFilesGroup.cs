// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.SystemTab.Maintenance.Files
{
    [StartPageControl(StartPageMaintenancePage.Name, 1000)]
    public class StartPageFilesGroup : StartPageGroupBase
    {
        public const string Name = StartPageMaintenancePage.Name + ".Files";

        public StartPageFilesGroup([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Files";
            Description = "Clean up files on the web site.";
        }
    }
}
