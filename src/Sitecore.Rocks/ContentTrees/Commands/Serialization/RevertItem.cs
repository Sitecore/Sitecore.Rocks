// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Rules;

namespace Sitecore.Rocks.ContentTrees.Commands.Serialization
{
    [Command(Submenu = SerializationSubmenu.Name), RuleAction("revert item", "Serialization"), Feature(FeatureNames.Serialization)]
    public class RevertItem : SerializeItemCommand, IRuleAction
    {
        public RevertItem()
        {
            Text = Resources.RevertItem_RevertItem_Revert_Item;
            Group = "Item";
            SortingValue = 1200;

            SerializationOperation = SerializationOperation.Revert;
            SerializationText = Resources.RevertItem_RevertItem_Reverting_Item___;
            ConfirmationText = "Are you sure you want to revert the item(s)?";
        }

        protected override void Execute(ItemUri itemUri, ExecuteCompleted callback)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(callback, nameof(callback));

            itemUri.Site.DataService.RevertItem(itemUri, callback);
        }
    }
}
