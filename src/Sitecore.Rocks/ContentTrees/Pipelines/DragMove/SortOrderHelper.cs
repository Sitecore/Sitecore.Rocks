// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Pipelines.DragMove
{
    internal static class SortOrderHelper
    {
        public static void GetSortOrder([NotNull] ItemTreeViewItem target, ControlDragAdornerPosition position, int itemCount, out int sortOrder, out int sortOrderDelta, [CanBeNull] out ItemTreeViewItem parent, [CanBeNull] out ItemTreeViewItem anchor)
        {
            Debug.ArgumentNotNull(target, nameof(target));

            if (position == ControlDragAdornerPosition.Top)
            {
                GetBeforeSortOrder(target, itemCount, out sortOrder, out sortOrderDelta, out parent, out anchor);
                return;
            }

            GetAfterSortOrder(target, itemCount, out sortOrder, out sortOrderDelta, out parent, out anchor);
        }

        private static void GetAfterSortOrder([NotNull] ItemTreeViewItem target, int itemCount, out int sortOrder, out int sortOrderDelta, [CanBeNull] out ItemTreeViewItem parent, [CanBeNull] out ItemTreeViewItem anchor)
        {
            Debug.ArgumentNotNull(target, nameof(target));

            sortOrder = target.Item.SortOrder;
            parent = null;
            anchor = null;

            var itemsControl = target.GetParentTreeViewItem();
            if (itemsControl == null)
            {
                sortOrder += 100;
                sortOrderDelta = 100;
                return;
            }

            parent = itemsControl as ItemTreeViewItem;
            if (parent == null)
            {
                sortOrder -= itemCount * 100;
                sortOrderDelta = 100;
                return;
            }

            var index = itemsControl.Items.IndexOf(target);
            if (index < 0 || index == itemsControl.Items.Count - 1)
            {
                sortOrder += 100;
                sortOrderDelta = 100;
                return;
            }

            var nextItem = itemsControl.Items[index + 1] as ItemTreeViewItem;
            if (nextItem == null)
            {
                sortOrder += 100;
                sortOrderDelta = 100;
                return;
            }

            anchor = nextItem;

            var nextSortOrder = nextItem.Item.SortOrder;
            if (nextSortOrder == sortOrder)
            {
                var sort = 0;
                var fieldId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Appearance/Appearance/__Sortorder");

                for (var i = 0; i < parent.Items.Count; i++)
                {
                    var item = parent.Items[i] as ItemTreeViewItem;
                    if (item != null)
                    {
                        ItemModifier.Edit(item.Item.ItemUri, fieldId, sort.ToString());
                        item.Item.SortOrder = sort;
                    }

                    sort += i == index ? (itemCount + 1) * 100 : 100;
                }

                sortOrder = index * 100;
                nextSortOrder = sortOrder + (itemCount + 1) * 100;
            }

            var gap = nextSortOrder - sortOrder;
            sortOrderDelta = gap / (itemCount + 1);

            if (sortOrderDelta == 0)
            {
                sortOrder = sortOrder + (int)Math.Floor((double)gap / 2);
                return;
            }

            sortOrder += sortOrderDelta;
        }

        private static void GetBeforeSortOrder([NotNull] ItemTreeViewItem target, int itemCount, out int sortOrder, out int sortOrderDelta, [CanBeNull] out ItemTreeViewItem parent, [CanBeNull] out ItemTreeViewItem anchor)
        {
            Debug.ArgumentNotNull(target, nameof(target));

            sortOrder = target.Item.SortOrder;
            parent = null;
            anchor = target;

            var itemsControl = target.GetParentTreeViewItem();
            if (itemsControl == null)
            {
                sortOrder -= itemCount * 100;
                sortOrderDelta = 100;
                return;
            }

            parent = itemsControl as ItemTreeViewItem;
            if (parent == null)
            {
                sortOrder -= itemCount * 100;
                sortOrderDelta = 100;
                return;
            }

            var index = itemsControl.Items.IndexOf(target);
            if (index == 0)
            {
                sortOrder -= itemCount * 100;
                sortOrderDelta = 100;
                return;
            }

            var previousItem = itemsControl.Items[index - 1] as ItemTreeViewItem;
            if (previousItem == null)
            {
                sortOrder -= itemCount * 100;
                sortOrderDelta = 100;
                return;
            }

            var previousSortOrder = previousItem.Item.SortOrder;
            if (previousSortOrder == sortOrder)
            {
                var sort = 0;
                var fieldId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Appearance/Appearance/__Sortorder");

                for (var i = 0; i < parent.Items.Count; i++)
                {
                    var item = parent.Items[i] as ItemTreeViewItem;
                    if (item != null)
                    {
                        ItemModifier.Edit(item.Item.ItemUri, fieldId, sort.ToString());
                        item.Item.SortOrder = sort;
                    }

                    sort += i == index - 1 ? (itemCount + 1) * 100 : 100;
                }

                previousSortOrder = (index - 1) * 100;
                sortOrder = previousSortOrder + (itemCount + 1) * 100;
            }

            var gap = sortOrder - previousSortOrder;
            sortOrderDelta = gap / (itemCount + 1);

            if (sortOrderDelta == 0)
            {
                sortOrder = previousSortOrder + (int)Math.Ceiling((double)gap / 2);
                return;
            }

            sortOrder -= sortOrderDelta * itemCount;
        }
    }
}
