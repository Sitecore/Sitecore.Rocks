// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Tabs.DevelopingTab.Serialization.Database
{
    [StartPageControl(StartPageSerializationPage.Name, 1000)]
    public class StartPageDatabaseGroup : StartPageGroupBase
    {
        public const string Name = StartPageSerializationPage.Name + ".Serialize";

        public StartPageDatabaseGroup([NotNull] StartPageViewer startPage) : base(startPage, Name)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            Text = "Serialize";
            Description = "Serialize and update content items from the serialization files";
        }
    }
}
