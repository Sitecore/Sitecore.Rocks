// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.ContentTab.Publishing
{
    [StartPageControl(StartPageContentMainTab.Name, 3000)]
    public class StartPagePublishingPage : StartPageListBase
    {
        public const string Name = StartPageContentMainTab.Name + ".Publishing";

        public StartPagePublishingPage([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Publishing";
        }
    }
}
