// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.Configuring.WebConfig
{
    [StartPageControl(StartPageConfiguringPage.Name, 1000)]
    public class StartPageWebConfigGroup : StartPageGroupBase
    {
        public const string Name = StartPageConfiguringPage.Name + ".WebConfig";

        public StartPageWebConfigGroup([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Web Config";
            Description = "Explore settings and configuration in the web.config file.";
        }
    }
}
