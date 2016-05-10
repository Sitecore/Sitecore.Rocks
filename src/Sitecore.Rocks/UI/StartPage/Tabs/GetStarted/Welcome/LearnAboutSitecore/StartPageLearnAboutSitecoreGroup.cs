// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.Welcome.LearnAboutSitecore
{
    [StartPageControl(StartPageWelcomePage.Name, 2000)]
    public class StartPageLearnAboutSitecoreGroup : StartPageGroupBase
    {
        public const string Name = StartPageWelcomePage.Name + ".LearnAboutSitecore";

        public StartPageLearnAboutSitecoreGroup([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Learn About Sitecore";
            Description = "Get the latest information about the Sitecore product.";
        }
    }
}
