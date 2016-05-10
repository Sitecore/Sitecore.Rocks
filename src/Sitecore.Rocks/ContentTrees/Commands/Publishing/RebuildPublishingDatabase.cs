// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentTrees.Commands.Publishing
{
    [Command(Submenu = PublishSubmenu.Name), Feature(FeatureNames.Publishing)]
    public class RebuildPublishingDatabase : PublishDatabaseCommand
    {
        public RebuildPublishingDatabase()
        {
            Group = "PublishDatabase";
            Text = Resources.RebuildPublishingDatabase_RebuildPublishingDatabase_Rebuild_Publishing_Database;
            PublishingText = Resources.RebuildPublishingDatabase_RebuildPublishingDatabase_Rebuilding_Publishing_Database___;
            Mode = 3;
            SortingValue = 5300;
        }
    }
}
