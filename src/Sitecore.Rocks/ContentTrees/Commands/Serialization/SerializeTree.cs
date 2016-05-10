// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Rules;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Serialization
{
    [Command(Submenu = SerializationSubmenu.Name), RuleAction("serialize tree", "Serialization"), CommandId(CommandIds.SitecoreExplorer.SerializeTree, typeof(ContentTreeContext), ToolBar = ToolBars.SitecoreExplorer.Id, Group = ToolBars.SitecoreExplorer.Serializing, Priority = 0x0600, Icon = "Resources/16x16/items_serialize.png"), Feature(FeatureNames.Serialization)]
    public class SerializeTree : SerializeItemCommand, IRuleAction
    {
        public SerializeTree()
        {
            Text = Resources.SerializeTree_SerializeTree_Serialize_Tree;
            Group = "Tree";
            SortingValue = 2000;

            SerializationOperation = SerializationOperation.SerializeTree;
            SerializationText = Resources.SerializeTree_SerializeTree_Serializing_Tree___;
            ConfirmationText = "Are you sure you want to serialize the tree?";
        }

        protected override void Execute(ItemUri itemUri, ExecuteCompleted callback)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(callback, nameof(callback));

            itemUri.Site.DataService.SerializeTree(itemUri, callback);
        }
    }
}
