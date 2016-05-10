// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.SystemTab
{
    [StartPageControl(9000)]
    public class StartPageSystemMainTab : StartPageTabBase
    {
        public const string Name = "System";

        public StartPageSystemMainTab([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "System";
            TabStyle = TabStyle.Page;
        }
    }
}
