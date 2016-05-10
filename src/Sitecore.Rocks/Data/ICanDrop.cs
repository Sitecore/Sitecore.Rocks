// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;

namespace Sitecore.Rocks.Data
{
    public interface ICanDrop
    {
        void HandleDragOver([NotNull] object sender, [NotNull] DragEventArgs e, [NotNull] BaseTreeViewItem treeViewItem);

        void HandleDrop([NotNull] object sender, [NotNull] DragEventArgs e, [NotNull] BaseTreeViewItem treeViewItem);
    }
}
