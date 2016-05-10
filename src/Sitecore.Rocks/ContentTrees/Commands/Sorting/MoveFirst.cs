// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Sorting
{
    [Command(Submenu = "Sorting"), CommandId(CommandIds.SitecoreExplorer.MoveFirst, typeof(ContentTreeContext)), Feature(FeatureNames.Sorting)]
    public class MoveFirst : MoveBase
    {
        public MoveFirst()
        {
            Text = Resources.MoveFirst_MoveFirst_Move_First;
            SortingValue = 3000;
        }

        protected override int GetSortOrder(ItemTreeViewItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var parent = item.GetParentTreeViewItem();
            if (parent == null)
            {
                return item.Item.SortOrder;
            }

            var first = parent.Items[0] as ItemTreeViewItem;
            if (first == null)
            {
                return item.Item.SortOrder;
            }

            return first.Item.SortOrder - 100;
        }
    }
}
