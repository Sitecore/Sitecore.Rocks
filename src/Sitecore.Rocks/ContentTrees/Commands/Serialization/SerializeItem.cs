// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Rules;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Serialization
{
    [Command(Submenu = SerializationSubmenu.Name), RuleAction("serialize item", "Serialization"), CommandId(CommandIds.ItemEditor.SerializeItem, typeof(ContentEditorContext), ToolBar = ToolBars.ItemEditor.Id, Group = ToolBars.ItemEditor.Serializing, Priority = 0x0600, Icon = "Resources/16x16/item_serialize.png"), CommandId(CommandIds.SitecoreExplorer.SerializeItem, typeof(ContentTreeContext), ToolBar = ToolBars.SitecoreExplorer.Id, Group = ToolBars.SitecoreExplorer.Serializing, Priority = 0x0600, Icon = "Resources/16x16/item_serialize.png"), Feature(FeatureNames.Serialization)]
    public class SerializeItem : SerializeItemCommand, IRuleAction
    {
        public SerializeItem()
        {
            Text = Resources.SerializeItem_SerializeItem_Serialize_Item;
            Group = "Item";
            SortingValue = 1000;

            SerializationOperation = SerializationOperation.Serialize;
            SerializationText = Resources.SerializeItem_SerializeItem_Serializing_Item___;
            ConfirmationText = "Are you sure you want to serialize the item(s)?";
        }

        protected override void Execute(ItemUri itemUri, ExecuteCompleted callback)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(callback, nameof(callback));

            itemUri.Site.DataService.SerializeItem(itemUri, callback);
        }
    }
}
