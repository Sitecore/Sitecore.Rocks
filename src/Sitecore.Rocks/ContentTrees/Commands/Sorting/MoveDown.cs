// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Sorting
{
    [Command(Submenu = "Sorting"), CommandId(CommandIds.SitecoreExplorer.MoveDown, typeof(ContentTreeContext)), Feature(FeatureNames.Sorting)]
    public class MoveDown : MoveBase
    {
        public MoveDown()
        {
            Text = Resources.MoveDown_MoveDown_Move_Down;
            SortingValue = 2000;
            Icon = new Icon("Resources/16x16/down.png");
        }

        protected override int GetSortOrder(ItemTreeViewItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var parent = item.GetParentTreeViewItem();
            if (parent == null)
            {
                return item.Item.SortOrder;
            }

            var index = parent.Items.IndexOf(item);
            if (index < 0 || index == parent.Items.Count - 1)
            {
                return item.Item.SortOrder;
            }

            var next = parent.Items[index + 1] as ItemTreeViewItem;
            if (next == null)
            {
                return item.Item.SortOrder;
            }

            if (index == parent.Items.Count - 2)
            {
                return next.Item.SortOrder + 100;
            }

            var nextNext = parent.Items[index + 2] as ItemTreeViewItem;
            if (nextNext == null)
            {
                return item.Item.SortOrder;
            }

            return next.Item.SortOrder + (nextNext.Item.SortOrder - next.Item.SortOrder) / 2;
        }
    }
}
