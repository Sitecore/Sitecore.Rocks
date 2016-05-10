// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Rules;

namespace Sitecore.Rocks.ContentTrees.Commands.Serialization
{
    [Command(Submenu = SerializationSubmenu.Name), RuleAction("update tree serialization", "Serialization"), Feature(FeatureNames.Serialization)]
    public class UpdateTree : SerializeItemCommand, IRuleAction
    {
        public UpdateTree()
        {
            Text = Resources.UpdateTree_UpdateTree_Update_Tree;
            Group = "Tree";
            SortingValue = 2100;

            SerializationOperation = SerializationOperation.UpdateTree;
            SerializationText = Resources.UpdateTree_UpdateTree_Updating_Tree___;
            ConfirmationText = "Are you sure you want to update the tree?";
        }

        protected override void Execute(ItemUri itemUri, ExecuteCompleted callback)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(callback, nameof(callback));

            itemUri.Site.DataService.UpdateTree(itemUri, callback);
        }

        protected override void Executed(object parameter)
        {
            Debug.ArgumentNotNull(parameter, nameof(parameter));

            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return;
            }

            var item = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (item == null)
            {
                return;
            }

            item.Refresh();
        }
    }
}
