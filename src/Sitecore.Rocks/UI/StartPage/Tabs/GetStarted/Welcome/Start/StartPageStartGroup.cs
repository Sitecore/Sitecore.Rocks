// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.Welcome.Start
{
    [StartPageControl(StartPageWelcomePage.Name, 1000)]
    public class StartPageStartGroup : StartPageGroupBase
    {
        public const string Name = StartPageWelcomePage.Name + ".Start";

        public StartPageStartGroup([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Start";
            Description = "Get started using Sitecore and Sitecore Rocks.";
        }
    }
}
