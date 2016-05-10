// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.ContentTab.Items.ContentItems
{
    [StartPageControl(StartPageItemsPage.Name, 2000)]
    public class StartPageSearchAndReplaceGroup : StartPageGroupBase
    {
        public const string Name = StartPageItemsPage.Name + ".SearchAndReplace";

        public StartPageSearchAndReplaceGroup([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Search and Replace";
            Description = "Search and replace fields in many items in one go.";
        }
    }
}
