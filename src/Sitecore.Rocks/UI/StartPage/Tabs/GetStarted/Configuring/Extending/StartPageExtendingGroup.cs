// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.Configuring.Extending
{
    [StartPageControl(StartPageConfiguringPage.Name, 9000)]
    public class StartPageExtendingGroup : StartPageGroupBase
    {
        public const string Name = StartPageConfiguringPage.Name + ".Extending";

        public StartPageExtendingGroup([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Extending";
            Description = "Add more power to Sitecore using shared source modules";
        }
    }
}
