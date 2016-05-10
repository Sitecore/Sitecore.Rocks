// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Publishing
{
    [Command(Submenu = PublishSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.IncrementalPublish, typeof(ContentTreeContext), ToolBar = ToolBars.SitecoreExplorer.Id, Group = ToolBars.SitecoreExplorer.Publishing, Priority = 0x0530, Icon = "Resources/16x16/database_publish.png"), Feature(FeatureNames.Publishing)]
    public class IncrementalPublish : PublishDatabaseCommand
    {
        public IncrementalPublish()
        {
            Group = "PublishDatabase";
            Text = Resources.IncrementalPublish_IncrementalPublish_Incremental_Publish;
            PublishingText = Resources.IncrementalPublish_IncrementalPublish_Publishing_Incremental___;
            Mode = 1;
            SortingValue = 5000;
            Icon = new Icon("Resources/16x16/publishing.png");
        }
    }
}
