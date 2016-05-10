// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Rules;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Publishing
{
    [Command(Submenu = PublishSubmenu.Name), RuleAction("publish item", "Publishing"), CommandId(CommandIds.SitecoreExplorer.PublishItem, typeof(ContentTreeContext), ToolBar = ToolBars.SitecoreExplorer.Id, Group = ToolBars.SitecoreExplorer.Publishing, Priority = 0x0500, Icon = "Resources/16x16/item_publish.png"), CommandId(CommandIds.ItemEditor.PublishItem, typeof(ContentEditorContext), ToolBar = ToolBars.ItemEditor.Id, Group = ToolBars.ItemEditor.Publishing, Priority = 0x0100, Icon = "Resources/16x16/item_publish.png"), Feature(FeatureNames.Publishing)]
    public class PublishItem : PublishItemCommand, IRuleAction
    {
        public PublishItem()
        {
            Group = "PublishItem";
            Text = Resources.PublishItem_PublishItem_Publish_Item;
            PublishingText = Resources.PublishItem_PublishItem_Publishing_Item___;
            Deep = false;
            CompareRevisisions = false;
            SortingValue = 1000;
        }
    }
}
