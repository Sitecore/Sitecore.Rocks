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
    [Command(Submenu = SerializationSubmenu.Name), RuleAction("revert tree", "Serialization"), Feature(FeatureNames.Serialization)]
    public class RevertTree : SerializeItemCommand, IRuleAction
    {
        public RevertTree()
        {
            Text = Resources.RevertTree_RevertTree_Revert_Tree;
            Group = "Tree";
            SortingValue = 2200;

            SerializationOperation = SerializationOperation.RevertTree;
            SerializationText = Resources.RevertTree_RevertTree_Reverting_Tree___;
            ConfirmationText = "Are you sure you want to revert the tree?";
        }

        protected override void Execute(ItemUri itemUri, ExecuteCompleted callback)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(callback, nameof(callback));

            itemUri.Site.DataService.RevertTree(itemUri, callback);
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
