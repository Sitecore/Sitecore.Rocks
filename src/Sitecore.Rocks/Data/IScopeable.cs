// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;

namespace Sitecore.Rocks.Data
{
    public interface IScopeable
    {
        [NotNull]
        BaseTreeViewItem GetScopedTreeViewItem();
    }
}
