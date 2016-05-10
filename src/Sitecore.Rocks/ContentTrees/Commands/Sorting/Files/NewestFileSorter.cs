// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Commands.Sorting.Files
{
    [FileSort("Newest on Top")]
    public class NewestFileSorter : IFileSorter, IComparer<FileTreeViewItem>
    {
        public int Compare([NotNull] FileTreeViewItem x, [NotNull] FileTreeViewItem y)
        {
            Assert.ArgumentNotNull(x, nameof(x));
            Assert.ArgumentNotNull(y, nameof(y));

            return y.Updated.CompareTo(x.Updated);
        }

        public void Sort(List<FileTreeViewItem> items)
        {
            Assert.ArgumentNotNull(items, nameof(items));

            items.Sort(this);
        }
    }
}
