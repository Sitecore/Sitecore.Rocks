// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.UI.KeyboardSchemes;

namespace Sitecore.Rocks.Controls
{
    public class MultiSelectTreeView : TreeView
    {
        private readonly List<BaseTreeViewItem> _selectedItems = new List<BaseTreeViewItem>();

        private BaseTreeViewItem _lastSelectedItem;

        private Point _origin;

        public MultiSelectTreeView()
        {
            _origin.X = double.MinValue;
            _origin.Y = double.MinValue;
            Background = Brushes.Transparent;
        }

        public bool AllowDrag { get; set; }

        [NotNull]
        public List<BaseTreeViewItem> SelectedItems
        {
            get { return _selectedItems; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                Clear();

                _selectedItems.AddRange(value);

                foreach (var baseTreeViewItem in _selectedItems)
                {
                    baseTreeViewItem.IsItemSelected = true;
                }
            }
        }

        public SelectionMode SelectionMode { get; set; }

        public void Clear()
        {
            var treeViewItem = SelectedItem as TreeViewItem;
            if (treeViewItem != null)
            {
                treeViewItem.IsSelected = false;
            }

            foreach (var baseTreeViewItem in _selectedItems)
            {
                baseTreeViewItem.IsItemSelected = false;
            }

            _selectedItems.Clear();
        }

        [NotNull]
        public IEnumerable<BaseTreeViewItem> GetAllItems()
        {
            return GetAllItems(Items);
        }

        [CanBeNull]
        public BaseTreeViewItem GetBaseTreeViewItem([NotNull] object source)
        {
            Assert.ArgumentNotNull(source, nameof(source));

            var control = source as FrameworkElement;

            while (control != null && !(control is BaseTreeViewItem))
            {
                control = control.Parent as FrameworkElement;
            }

            return control as BaseTreeViewItem;
        }

        [NotNull]
        public IEnumerable<BaseTreeViewItem> GetSelectedItems([CanBeNull] object parameter)
        {
            var item = parameter as BaseTreeViewItem;
            if (item != null)
            {
                if (SelectedItems.Any(i => i.Equals(item)))
                {
                    return SelectedItems;
                }

                SelectItem(item, true);

                return new List<BaseTreeViewItem>
                {
                    item
                };
            }

            return SelectedItems;
        }

        public event RoutedPropertyChangedEventHandler<object> SelectedItemsChanged;

        protected override void OnInitialized([NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            base.OnInitialized(e);

            DragOver += HandleDragOver;
            Drop += HandleDrop;
            PreviewMouseLeftButtonDown += HandleMouseLeftButtonDown;
            MouseMove += HandleMouseMove;
        }

        protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            var selected = e.NewValue as BaseTreeViewItem;
            if (selected == null)
            {
                return;
            }

            e.Handled = true;

            SelectItem(selected, true);
        }

        internal void Select([NotNull] BaseTreeViewItem selected)
        {
            Debug.ArgumentNotNull(selected, nameof(selected));

            if (selected.Visibility == Visibility.Collapsed)
            {
                return;
            }

            selected.IsItemSelected = true;
            if (!SelectedItems.Contains(selected))
            {
                SelectedItems.Add(selected);
            }
        }

        [NotNull]
        private IEnumerable<BaseTreeViewItem> GetAllItems([NotNull] ItemCollection items)
        {
            foreach (var item in items.OfType<BaseTreeViewItem>())
            {
                yield return item;

                foreach (var child in GetAllItems(item.Items))
                {
                    yield return child;
                }
            }
        }

        [CanBeNull]
        private TreeViewItem GetTreeViewItem([CanBeNull] object source)
        {
            var element = source as FrameworkElement;
            if (element == null)
            {
                return null;
            }

            return element.GetAncestorOrSelf<TreeViewItem>();
        }

        private void HandleDragOver([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Effects = DragDropEffects.None;
            e.Handled = true;

            var target = GetTreeViewItem(e.Source) as ICanDrop;
            if (target == null)
            {
                return;
            }

            var treeViewItem = target as BaseTreeViewItem;
            if (treeViewItem == null)
            {
                return;
            }

            target.HandleDragOver(sender, e, treeViewItem);
        }

        private void HandleDrop([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Handled = true;

            var target = GetTreeViewItem(e.Source) as ICanDrop;
            if (target == null)
            {
                return;
            }

            var treeViewItem = GetTreeViewItem(e.Source) as BaseTreeViewItem;
            if (treeViewItem == null)
            {
                return;
            }

            target.HandleDrop(sender, e, treeViewItem);
        }

        private void HandleMouseLeftButtonDown([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            DragManager.HandleMouseDown(this, e, out _origin);
        }

        private void HandleMouseMove([NotNull] object sender, [NotNull] MouseEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!DragManager.IsDragStart(this, e, ref _origin))
            {
                return;
            }

            if (!AllowDrag)
            {
                return;
            }

            var items = SelectedItems;
            if (items.Count == 0)
            {
                return;
            }

            string dragIdentifier = null;
            foreach (var item in items)
            {
                var draggable = item as ICanDrag;
                if (draggable == null)
                {
                    return;
                }

                if (dragIdentifier == null)
                {
                    dragIdentifier = draggable.GetDragIdentifier();
                }
                else if (dragIdentifier != draggable.GetDragIdentifier())
                {
                    return;
                }
            }

            if (string.IsNullOrEmpty(dragIdentifier))
            {
                return;
            }

            var dragData = new DataObject(dragIdentifier, items);

            DragManager.SetData(dragData, items);
            DragManager.DoDragDrop(this, dragData, DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);

            _origin.X = double.MinValue;
            _origin.Y = double.MinValue;

            e.Handled = true;
        }

        private void SelectItem([NotNull] BaseTreeViewItem selected, bool acceptKeys)
        {
            Debug.ArgumentNotNull(selected, nameof(selected));

            if (SelectionMode == SelectionMode.Single)
            {
                Clear();
                selected.IsSelected = true;
            }
            else
            {
                var isShift = KeyboardManager.IsActive == 0 && acceptKeys && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift));
                var isCtrl = KeyboardManager.IsActive == 0 && acceptKeys && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));

                if (!isCtrl && !isShift && SelectedItems.Count > 1 && selected.IsItemSelected)
                {
                    selected.IsSelected = true;
                }
                else
                {
                    if (!isCtrl)
                    {
                        SelectedItems.ForEach(delegate(BaseTreeViewItem f)
                        {
                            f.IsSelected = false;
                            f.IsItemSelected = false;
                        });
                        SelectedItems.Clear();

                        if (!isShift)
                        {
                            _lastSelectedItem = null;
                        }
                    }

                    if (isShift)
                    {
                        ShiftSelect(selected);
                    }
                    else
                    {
                        SingleSelect(selected);
                    }
                }
            }

            var changed = SelectedItemsChanged;
            if (changed != null)
            {
                changed(this, new RoutedPropertyChangedEventArgs<object>(null, null));
            }
        }

        private void ShiftSelect([NotNull] BaseTreeViewItem selected)
        {
            Debug.ArgumentNotNull(selected, nameof(selected));

            var baseTreeViewItem = _lastSelectedItem;
            if (baseTreeViewItem == null)
            {
                return;
            }

            var select = false;

            ShiftSelect(Items, selected, baseTreeViewItem, ref select);
        }

        private void ShiftSelect([NotNull] ItemCollection items, [NotNull] BaseTreeViewItem item0, [NotNull] BaseTreeViewItem item1, ref bool select)
        {
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(item0, nameof(item0));
            Debug.ArgumentNotNull(item1, nameof(item1));

            foreach (var i in items)
            {
                var item = i as BaseTreeViewItem;
                if (item == null)
                {
                    continue;
                }

                if (item.Equals(item0) || item.Equals(item1))
                {
                    select = !select;
                    Select(item);
                }
                else if (select)
                {
                    Select(item);
                }

                ShiftSelect(item.Items, item0, item1, ref select);
            }
        }

        private void SingleSelect([NotNull] BaseTreeViewItem selected)
        {
            Debug.ArgumentNotNull(selected, nameof(selected));

            if (selected.IsItemSelected)
            {
                selected.IsItemSelected = false;
                SelectedItems.Remove(selected);
            }
            else
            {
                Select(selected);
                _lastSelectedItem = selected;
            }
        }
    }
}
