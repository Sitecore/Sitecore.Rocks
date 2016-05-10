// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Publishing
{
    [Command(Submenu = PublishSubmenu.Name), CommandId(CommandIds.ItemEditor.PublishItemAndDependencies, typeof(ContentEditorContext), ToolBar = ToolBars.ItemEditor.Id, Group = ToolBars.ItemEditor.Publishing, Priority = 0x0500, Icon = "Resources/16x16/items_publish.png"), CommandId(CommandIds.SitecoreExplorer.PublishItemAndDependencies, typeof(ContentTreeContext), ToolBar = ToolBars.SitecoreExplorer.Id, Group = ToolBars.SitecoreExplorer.Publishing, Priority = 0x0520, Icon = "Resources/16x16/items_publish.png"), Feature(FeatureNames.Publishing)]
    public class PublishItemAndDependencies : PublishItemAndDependenciesCommand
    {
        public PublishItemAndDependencies()
        {
            Text = Resources.PublishItemReferences_PublishItemReferences_Publish_Item_and_References___;
            Group = "PublishItem";
            PublishingText = Resources.PublishItemAndDependencies_PublishItemAndDependencies_Publishing_Item_and_Dependencies___;
            Deep = false;
            CompareRevisisions = false;
            SortingValue = 1200;
        }
    }
}
