// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;

namespace Sitecore.Rocks.ContentTrees.VirtualItems
{
    public interface IVirtualItem
    {
        bool CanAddItem([CanBeNull] BaseTreeViewItem parent);

        [NotNull]
        BaseTreeViewItem GetItem([CanBeNull] BaseTreeViewItem parent);
    }
}
