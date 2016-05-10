// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.DevelopingTab
{
    [StartPageControl(3000)]
    public class StartPageDevelopingMainTab : StartPageTabBase
    {
        public const string Name = "Developing";

        public StartPageDevelopingMainTab([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Developing";
            TabStyle = TabStyle.Page;
        }
    }
}
