// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.ContentTab.Media
{
    [StartPageControl(StartPageContentMainTab.Name, 2000)]
    public class StartPageMediaPage : StartPageListBase
    {
        public const string Name = StartPageContentMainTab.Name + ".Media";

        public StartPageMediaPage([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Media";
        }
    }
}
