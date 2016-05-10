// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.Welcome.SendFeedback
{
    [StartPageControl(StartPageWelcomePage.Name, 3000)]
    public class StartPageSendFeedbackGroup : StartPageGroupBase
    {
        public const string Name = StartPageWelcomePage.Name + ".SendFeedback";

        public StartPageSendFeedbackGroup([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Send Feedback";
            Description = "Report bugs or ask for help.";
        }
    }
}
