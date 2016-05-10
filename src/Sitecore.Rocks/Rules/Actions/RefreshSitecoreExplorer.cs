// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI;

namespace Sitecore.Rocks.Rules.Actions
{
    [RuleAction("refresh Sitecore Explorer", "System")]
    public class RefreshSitecoreExplorer : RuleAction
    {
        public RefreshSitecoreExplorer()
        {
            Text = "Refresh Sitecore Explorer";
        }

        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            Assert.ArgumentNotNull(parameter, nameof(parameter));

            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var tree = ActiveContext.ActiveContentTree;
            if (tree == null)
            {
                return;
            }

            foreach (var item in context.Items)
            {
                var baseTreeViewItem = tree.ItemTreeView.FindItem<ItemTreeViewItem>(item.ItemUri);
                if (baseTreeViewItem == null)
                {
                    continue;
                }

                baseTreeViewItem.Refresh();
            }
        }
    }
}
