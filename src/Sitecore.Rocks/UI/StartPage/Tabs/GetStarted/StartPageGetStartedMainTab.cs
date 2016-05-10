// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted
{
    [StartPageControl(1000)]
    public class StartPageGetStartedMainTab : StartPageTabBase
    {
        public const string Name = "GetStarted";

        public StartPageGetStartedMainTab([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Get Started";
            TabStyle = TabStyle.Page;
        }
    }
}
