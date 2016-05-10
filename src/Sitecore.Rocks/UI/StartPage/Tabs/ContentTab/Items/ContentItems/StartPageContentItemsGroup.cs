// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.ContentTab.Items.ContentItems
{
    [StartPageControl(StartPageItemsPage.Name, 1000)]
    public class StartPageContentItemsGroup : StartPageGroupBase
    {
        public const string Name = StartPageItemsPage.Name + ".ContentItems";

        public StartPageContentItemsGroup([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Content Items";
            Description = "Edit and manage content items.";
        }
    }
}
