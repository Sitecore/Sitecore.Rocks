// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Rules;

namespace Sitecore.Rocks.ContentTrees.Commands.Serialization
{
    [Command(Submenu = SerializationSubmenu.Name), RuleAction("update item serialization", "Serialization"), Feature(FeatureNames.Serialization)]
    public class UpdateItem : SerializeItemCommand, IRuleAction
    {
        public UpdateItem()
        {
            Text = Resources.UpdateItem_UpdateItem_Update_Item;
            Group = "Item";
            SortingValue = 1100;

            SerializationOperation = SerializationOperation.Update;
            SerializationText = Resources.UpdateItem_UpdateItem_Updating_Item___;
            ConfirmationText = "Are you sure you want to update the item(s)?";
        }

        protected override void Execute(ItemUri itemUri, ExecuteCompleted callback)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(callback, nameof(callback));

            itemUri.Site.DataService.UpdateItem(itemUri, callback);
        }
    }
}
