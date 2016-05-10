// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.Welcome
{
    [StartPageControl(StartPageGetStartedMainTab.Name, 1000)]
    public class StartPageWelcomePage : StartPageListBase
    {
        public const string Name = StartPageGetStartedMainTab.Name + ".Welcome";

        public StartPageWelcomePage([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Welcome";
        }
    }
}
