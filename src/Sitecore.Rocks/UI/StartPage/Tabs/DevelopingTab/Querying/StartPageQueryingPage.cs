// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.DevelopingTab.Querying
{
    [StartPageControl(StartPageDevelopingMainTab.Name, 1000)]
    public class StartPageQueryingPage : StartPageListBase
    {
        public const string Name = StartPageDevelopingMainTab.Name + ".Querying";

        public StartPageQueryingPage([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Querying";
        }
    }
}
