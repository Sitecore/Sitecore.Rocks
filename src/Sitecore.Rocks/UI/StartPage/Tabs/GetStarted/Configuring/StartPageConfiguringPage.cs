// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.Configuring
{
    [StartPageControl(StartPageGetStartedMainTab.Name, 2000)]
    public class StartPageConfiguringPage : StartPageListBase
    {
        public const string Name = StartPageGetStartedMainTab.Name + ".Configuring";

        public StartPageConfiguringPage([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Configuring";
        }
    }
}
