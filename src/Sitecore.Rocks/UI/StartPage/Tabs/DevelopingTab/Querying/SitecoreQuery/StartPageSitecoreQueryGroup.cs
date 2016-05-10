// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.DevelopingTab.Querying.SitecoreQuery
{
    [StartPageControl(StartPageQueryingPage.Name, 1000)]
    public class StartPageSitecoreQueryGroup : StartPageGroupBase
    {
        public const string Name = StartPageQueryingPage.Name + ".SitecoreQuery";

        public StartPageSitecoreQueryGroup([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Sitecore Query and XPath";
            Description = "Use Sitecore Query or XPath to retrieve items from the databases";
        }
    }
}
