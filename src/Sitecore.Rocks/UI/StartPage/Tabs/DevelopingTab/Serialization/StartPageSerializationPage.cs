// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.DevelopingTab.Serialization
{
    [StartPageControl(StartPageDevelopingMainTab.Name, 3000)]
    public class StartPageSerializationPage : StartPageListBase
    {
        public const string Name = StartPageDevelopingMainTab.Name + ".Serialization";

        public StartPageSerializationPage([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Serialization";
        }
    }
}
