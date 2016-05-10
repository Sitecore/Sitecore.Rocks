// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Sorting
{
    [Command(Submenu = "Sorting"), CommandId(CommandIds.SitecoreExplorer.MoveLast, typeof(ContentTreeContext)), Feature(FeatureNames.Sorting)]
    public class MoveLast : MoveBase
    {
        public MoveLast()
        {
            Text = Resources.MoveLast_MoveLast_Move_Last;
            SortingValue = 4000;
        }

        protected override int GetSortOrder(ItemTreeViewItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var parent = item.GetParentTreeViewItem();
            if (parent == null)
            {
                return item.Item.SortOrder;
            }

            var last = parent.Items[parent.Items.Count - 1] as ItemTreeViewItem;
            if (last == null)
            {
                return item.Item.SortOrder;
            }

            return last.Item.SortOrder + 100;
        }
    }
}
