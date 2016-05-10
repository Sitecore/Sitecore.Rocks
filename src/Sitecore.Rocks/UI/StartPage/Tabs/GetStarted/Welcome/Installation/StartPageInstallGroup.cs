// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.Welcome.Installation
{
    [StartPageControl(StartPageWelcomePage.Name, 500)]
    public class StartPageInstallGroup : StartPageGroupBase
    {
        public const string Name = StartPageWelcomePage.Name + ".Installation";

        public StartPageInstallGroup([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Installation";
            Description = "Initial setup of a Sitecore web site";
        }
    }
}
