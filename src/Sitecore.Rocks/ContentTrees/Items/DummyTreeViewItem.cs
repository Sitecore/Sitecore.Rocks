// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.ContentTrees.Items
{
    public class DummyTreeViewItem
    {
        [NotNull]
        public static DummyTreeViewItem Instance { get; } = new DummyTreeViewItem();
    }
}
