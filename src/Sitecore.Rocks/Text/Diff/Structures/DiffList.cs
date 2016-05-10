// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Text.Diff.Structures
{
    public class DiffList<T> : IDiffList
    {
        public delegate IComparable GetsComparable(T item);

        private readonly GetsComparable getComparable;

        private readonly List<T> items;

        public DiffList([NotNull] List<T> items, [NotNull] GetsComparable getComparable)
        {
            Assert.ArgumentNotNull(items, nameof(items));
            Assert.ArgumentNotNull(getComparable, nameof(getComparable));

            this.items = items;
            this.getComparable = getComparable;
        }

        public int Count()
        {
            return items.Count();
        }

        [NotNull]
        public IComparable GetByIndex(int index)
        {
            return getComparable(items[index]);
        }
    }
}
