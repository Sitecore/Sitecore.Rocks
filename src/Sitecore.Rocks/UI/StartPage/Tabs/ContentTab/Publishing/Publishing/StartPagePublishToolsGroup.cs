// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.ContentTab.Publishing.Publishing
{
    [StartPageControl(StartPagePublishingPage.Name, 2000)]
    public class StartPagePublishToolsGroup : StartPageGroupBase
    {
        public const string Name = StartPagePublishingPage.Name + ".PublishInformation";

        public StartPagePublishToolsGroup([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Publish Information";
            Description = "View information about publishing.";
        }
    }
}
