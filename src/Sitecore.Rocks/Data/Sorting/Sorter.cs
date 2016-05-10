// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.IEnumerableExtensions;

namespace Sitecore.Rocks.Data.Sorting
{
    public class Sorter
    {
        public void InsertAfter([NotNull] IEnumerable<IHasSortOrder> elements, [NotNull] IHasSortOrder anchor, [NotNull] IEnumerable<IHasSortOrder> insertItems)
        {
            Assert.ArgumentNotNull(elements, nameof(elements));
            Assert.ArgumentNotNull(anchor, nameof(anchor));
            Assert.ArgumentNotNull(insertItems, nameof(insertItems));

            var list = elements.Where(e => !insertItems.Contains(e));

            if (!list.Contains(anchor))
            {
                throw Exceptions.InvalidOperation(Resources.Sorter_InsertAfter_Anchor_is_not_a_part_of_elements);
            }

            var next = list.Next(anchor);
            if (next == null)
            {
                AppendToEnd(insertItems, anchor);
                return;
            }

            if (IsUnsorted(list))
            {
                ApplyDefaultSorting(list);
            }

            var delta = (next.SortOrder - anchor.SortOrder) / (insertItems.Count() + 1);
            if (delta == 0)
            {
                Resort(list, anchor, next.SortOrder + insertItems.Count() * 100 + 200);
                delta = 100;
            }

            var value = anchor.SortOrder + delta;
            foreach (var item in insertItems)
            {
                item.SortOrder = value;
                value += delta;
            }
        }

        public void InsertBefore([NotNull] IEnumerable<IHasSortOrder> elements, [NotNull] IHasSortOrder anchor, [NotNull] IEnumerable<IHasSortOrder> insertItems)
        {
            Assert.ArgumentNotNull(elements, nameof(elements));
            Assert.ArgumentNotNull(anchor, nameof(anchor));
            Assert.ArgumentNotNull(insertItems, nameof(insertItems));

            var list = elements.Where(e => !insertItems.Contains(e));

            if (!list.Contains(anchor))
            {
                throw Exceptions.InvalidOperation(Resources.Sorter_InsertBefore_Anchor_is_not_a_part_of_elements);
            }

            var previous = list.Previous(anchor);
            if (previous != null)
            {
                InsertAfter(elements, previous, insertItems);
                return;
            }

            if (IsUnsorted(list))
            {
                ApplyDefaultSorting(list);
            }

            var value = anchor.SortOrder - insertItems.Count() * 100;

            foreach (var item in insertItems)
            {
                item.SortOrder = value;
                value += 100;
            }
        }

        private void AppendToEnd([NotNull] IEnumerable<IHasSortOrder> insertItems, [NotNull] IHasSortOrder last)
        {
            Assert.ArgumentNotNull(insertItems, nameof(insertItems));
            Assert.ArgumentNotNull(last, nameof(last));

            var value = last.SortOrder + 100;

            foreach (var item in insertItems)
            {
                item.SortOrder = value;
                value += 100;
            }
        }

        private void ApplyDefaultSorting([NotNull] IEnumerable<IHasSortOrder> list)
        {
            Assert.ArgumentNotNull(list, nameof(list));

            var value = 0;

            foreach (var item in list)
            {
                item.SortOrder = value;
                value += 100;
            }
        }

        private bool IsUnsorted([NotNull] IEnumerable<IHasSortOrder> list)
        {
            Assert.ArgumentNotNull(list, nameof(list));

            return list.Any(element => element.SortOrder != 0);
        }

        private void Resort([NotNull] IEnumerable<IHasSortOrder> list, [NotNull] IHasSortOrder anchor, int value)
        {
            Assert.ArgumentNotNull(list, nameof(list));
            Assert.ArgumentNotNull(anchor, nameof(anchor));

            list.Following(anchor, delegate(IHasSortOrder order)
            {
                order.SortOrder = value;
                value += 100;
            });
        }
    }
}
