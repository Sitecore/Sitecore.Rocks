// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.ContentTab.Items
{
    [StartPageControl(StartPageContentMainTab.Name, 1000)]
    public class StartPageItemsPage : StartPageListBase
    {
        public const string Name = StartPageContentMainTab.Name + ".Items";

        public StartPageItemsPage([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Items";
        }
    }
}
