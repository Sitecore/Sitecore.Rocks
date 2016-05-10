// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Sorting
{
    [Command(Submenu = "Sorting"), CommandId(CommandIds.SitecoreExplorer.MoveUp, typeof(ContentTreeContext)), Feature(FeatureNames.Sorting)]
    public class MoveUp : MoveBase
    {
        public MoveUp()
        {
            Text = Resources.MoveUp_MoveUp_Move_Up;
            SortingValue = 1000;
            Icon = new Icon("Resources/16x16/up.png");
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
            if (index <= 0)
            {
                return item.Item.SortOrder;
            }

            var previous = parent.Items[index - 1] as ItemTreeViewItem;
            if (previous == null)
            {
                return item.Item.SortOrder;
            }

            if (index == 1)
            {
                return previous.Item.SortOrder - 100;
            }

            var previousPrevious = parent.Items[index - 2] as ItemTreeViewItem;
            if (previousPrevious == null)
            {
                return item.Item.SortOrder;
            }

            return previousPrevious.Item.SortOrder + (previous.Item.SortOrder - previousPrevious.Item.SortOrder) / 2;
        }
    }
}
