// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;

namespace Sitecore.Rocks.ContentTrees.Commands.Sorting.Files
{
    public interface IFileSorter
    {
        void Sort([NotNull] List<FileTreeViewItem> items);
    }
}
