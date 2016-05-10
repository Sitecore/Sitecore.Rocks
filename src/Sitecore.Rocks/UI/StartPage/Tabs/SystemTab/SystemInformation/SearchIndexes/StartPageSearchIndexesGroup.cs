// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.SystemTab.SystemInformation.SearchIndexes
{
    [StartPageControl(StartPageSystemPage.Name, 2000)]
    public class StartPageSearchIndexesGroup : StartPageGroupBase
    {
        public const string Name = StartPageSystemPage.Name + ".SearchIndexes";

        public StartPageSearchIndexesGroup([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Search Indexes";
            Description = "Manage Lucene search indexes.";
        }
    }
}
