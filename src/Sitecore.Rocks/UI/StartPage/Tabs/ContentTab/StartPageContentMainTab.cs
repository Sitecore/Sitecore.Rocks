// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.ContentTab
{
    [StartPageControl(2000)]
    public class StartPageContentMainTab : StartPageTabBase
    {
        public const string Name = "Content";

        public StartPageContentMainTab([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Content";
            TabStyle = TabStyle.Page;
        }
    }
}
