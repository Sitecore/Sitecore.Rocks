// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Commands.Sorting.Files
{
    [FileSort("Name")]
    public class NameFileSorter : IFileSorter, IComparer<FileTreeViewItem>
    {
        public int Compare([NotNull] FileTreeViewItem x, [NotNull] FileTreeViewItem y)
        {
            Assert.ArgumentNotNull(x, nameof(x));
            Assert.ArgumentNotNull(y, nameof(y));

            var f1 = Path.GetFileName(x.FileUri.FileName) ?? string.Empty;
            var f2 = Path.GetFileName(y.FileUri.FileName) ?? string.Empty;

            return string.Compare(f1, f2, StringComparison.InvariantCultureIgnoreCase);
        }

        public void Sort(List<FileTreeViewItem> items)
        {
            Assert.ArgumentNotNull(items, nameof(items));

            items.Sort(this);
        }
    }
}
