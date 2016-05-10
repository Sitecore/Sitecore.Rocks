// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.Configuring.SystemItems
{
    [StartPageControl(StartPageConfiguringPage.Name, 2000)]
    public class StartPageSystemItemsGroup : StartPageGroupBase
    {
        public const string Name = StartPageConfiguringPage.Name + ".SystemItems";

        public StartPageSystemItemsGroup([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "System Items";
            Description = "Explore and manage system items in the Sitecore databases.";
        }
    }
}
