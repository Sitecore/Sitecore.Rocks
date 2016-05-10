// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Rules;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Publishing
{
    [Command(Submenu = PublishSubmenu.Name), RuleAction("publish item and subitems", "Publishing"), CommandId(CommandIds.SitecoreExplorer.PublishItemAndSubitems, typeof(ContentTreeContext), ToolBar = ToolBars.SitecoreExplorer.Id, Group = ToolBars.SitecoreExplorer.Publishing, Priority = 0x0510, Icon = "Resources/16x16/items_publish.png"), Feature(FeatureNames.Publishing)]
    public class PublishSubItems : PublishItemCommand, IRuleAction
    {
        public PublishSubItems()
        {
            Group = "PublishItem";
            Text = Resources.PublishSubItems_PublishSubItems_Publish_Item_and_Subitems;
            PublishingText = Resources.PublishSubItems_PublishSubItems_Publishing_Item_and_Subitems___;
            Deep = true;
            CompareRevisisions = false;
            SortingValue = 1300;
        }
    }
}
