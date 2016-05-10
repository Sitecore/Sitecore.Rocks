// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.ContentTab.Media.MediaLibrary
{
    [StartPageControl(StartPageMediaPage.Name, 1000)]
    public class StartPageMediaLibraryGroup : StartPageGroupBase
    {
        public const string Name = StartPageMediaPage.Name + ".MediaLibrary";

        public StartPageMediaLibraryGroup([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Media Library";
            Description = "Manage media assets";
        }
    }
}
