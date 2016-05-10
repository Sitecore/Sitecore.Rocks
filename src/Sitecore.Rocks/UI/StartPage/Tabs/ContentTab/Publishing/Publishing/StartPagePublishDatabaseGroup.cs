// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.ContentTab.Publishing.Publishing
{
    [StartPageControl(StartPagePublishingPage.Name, 1000)]
    public class StartPagePublishDatabaseGroup : StartPageGroupBase
    {
        public const string Name = StartPagePublishingPage.Name + ".Publish";

        public StartPagePublishDatabaseGroup([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Publish";
            Description = "Publishing the content to the web site.";
        }
    }
}
