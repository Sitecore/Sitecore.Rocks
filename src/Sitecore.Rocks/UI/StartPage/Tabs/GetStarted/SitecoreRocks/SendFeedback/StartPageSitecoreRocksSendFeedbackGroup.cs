// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.SitecoreRocks.SendFeedback
{
    [StartPageControl(StartPageSitecoreRocksPage.Name, 9000)]
    public class StartPageSitecoreRocksSendFeedbackGroup : StartPageGroupBase
    {
        public const string Name = StartPageSitecoreRocksPage.Name + ".SendFeedback";

        public StartPageSitecoreRocksSendFeedbackGroup([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Send Feedback";
            Description = "Report bugs or ask for help.";
        }
    }
}
