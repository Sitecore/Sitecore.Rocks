// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.Extensions.TreeViewItemExtensions
{
    public static class TreeViewItemExtensions
    {
        public static void FilterTreeViewItems([NotNull] this TreeView treeView, [NotNull] string filterText, [NotNull] Func<TreeViewItem, string> getText)
        {
            Assert.ArgumentNotNull(treeView, nameof(treeView));
            Assert.ArgumentNotNull(filterText, nameof(filterText));
            Assert.ArgumentNotNull(getText, nameof(getText));

            foreach (TreeViewItem item in treeView.Items)
            {
                if (item == null)
                {
                    continue;
                }

                FilterTreeViewItems(item, filterText, getText);
            }
        }

        [CanBeNull]
        public static ItemCollection FindPath([NotNull] this TreeView treeView, [NotNull] IEnumerable<TreeViewPathItem> path, [NotNull] Func<TreeViewPathItem, TreeViewItem> getTreeViewItem)
        {
            Assert.ArgumentNotNull(treeView, nameof(treeView));
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(getTreeViewItem, nameof(getTreeViewItem));

            var collection = treeView.Items;

            foreach (var pathPart in path)
            {
                TreeViewItem owner = null;

                foreach (var obj in collection)
                {
                    var item = obj as TreeViewItem;
                    if (item == null)
                    {
                        continue;
                    }

                    if (item.Tag as string != pathPart.Tag)
                    {
                        continue;
                    }

                    owner = item;
                }

                if (owner == null)
                {
                    owner = getTreeViewItem(pathPart);

                    collection.Add(owner);
                }

                collection = owner.Items;
            }

            return collection;
        }

        [CanBeNull]
        public static TreeViewItem Next([NotNull] this TreeViewItem treeViewItem)
        {
            Assert.ArgumentNotNull(treeViewItem, nameof(treeViewItem));

            var parent = treeViewItem.Parent as ItemsControl;
            if (parent == null)
            {
                return null;
            }

            var index = parent.Items.IndexOf(treeViewItem);
            if (index >= parent.Items.Count - 1)
            {
                return null;
            }

            return parent.Items[index + 1] as TreeViewItem;
        }

        [CanBeNull]
        public static TreeViewItem Previous([NotNull] this TreeViewItem treeViewItem)
        {
            Assert.ArgumentNotNull(treeViewItem, nameof(treeViewItem));

            var parent = treeViewItem.Parent as ItemsControl;
            if (parent == null)
            {
                return null;
            }

            var index = parent.Items.IndexOf(treeViewItem);
            if (index <= 0)
            {
                return null;
            }

            return parent.Items[index - 1] as TreeViewItem;
        }

        [CanBeNull]
        public static DependencyObject VisualUpwardSearch<T>([CanBeNull] DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
            {
                source = VisualTreeHelper.GetParent(source);
            }

            return source;
        }

        private static Visibility FilterTreeViewItems([NotNull] TreeViewItem item, [NotNull] string filterText, [NotNull] Func<TreeViewItem, string> getText)
        {
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(filterText, nameof(filterText));
            Debug.ArgumentNotNull(getText, nameof(getText));

            var isVisible = false;

            var text = getText(item);

            if (item.Items.Count == 0)
            {
                isVisible = text.IsFilterMatch(filterText);
            }
            else
            {
                foreach (var o in item.Items)
                {
                    var child = o as TreeViewItem;
                    if (child == null)
                    {
                        continue;
                    }

                    var v = FilterTreeViewItems(child, filterText, getText);

                    if (v == Visibility.Visible)
                    {
                        isVisible = true;
                    }
                }

                isVisible |= text.IsFilterMatch(filterText);
            }

            item.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;

            return item.Visibility;
        }
    }
}
