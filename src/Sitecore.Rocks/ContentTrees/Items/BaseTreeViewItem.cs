// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Gutters;
using Sitecore.Rocks.ContentTrees.Pipelines.GetChildren;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.ContentTrees.Items
{
    public abstract class BaseTreeViewItem : TreeViewItem
    {
        public delegate void GetChildrenDelegate(IEnumerable<BaseTreeViewItem> items);

        public static readonly DependencyProperty GutterImageProperty = DependencyProperty.Register(@"GutterImage", typeof(ImageSource), typeof(BaseTreeViewItem));

        public static readonly DependencyProperty GutterToolTipProperty = DependencyProperty.Register(@"GutterToolTip", typeof(object), typeof(BaseTreeViewItem));

        public static readonly DependencyProperty GutterVisibilityProperty = DependencyProperty.Register(@"GutterVisibility", typeof(Visibility), typeof(BaseTreeViewItem), new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty IsItemSelectedProperty = DependencyProperty.Register(@"IsItemSelected", typeof(bool), typeof(BaseTreeViewItem), new PropertyMetadata(false));

        protected BaseTreeViewItem()
        {
            var header = new BaseTreeViewItemHeader();
            header.TextChanged += SetText;

            Header = header;

            Expanded += Expand;
            Loaded += ControlLoaded;
        }

        [CanBeNull]
        public ImageSource GutterImage
        {
            get { return GetValue(GutterImageProperty) as ImageSource; }

            set { SetValue(GutterImageProperty, value); }
        }

        [CanBeNull]
        public object GutterToolTip
        {
            get { return GetValue(GutterToolTipProperty); }

            set { SetValue(GutterToolTipProperty, value); }
        }

        public Visibility GutterVisibility
        {
            get { return (Visibility)GetValue(GutterVisibilityProperty); }

            set { SetValue(GutterVisibilityProperty, value); }
        }

        public virtual bool HasChildren
        {
            get
            {
                if (Items.Count == 0)
                {
                    return false;
                }

                return Items.Count != 1 || Items[0] != DummyTreeViewItem.Instance;
            }
        }

        [NotNull]
        public Icon Icon
        {
            get { return ItemHeader.Icon; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                ItemHeader.Icon = value;
            }
        }

        public bool IsItemSelected
        {
            get { return (bool)GetValue(IsItemSelectedProperty); }

            set { SetValue(IsItemSelectedProperty, value); }
        }

        public bool IsLoading
        {
            get { return ItemHeader.IsLoading; }

            set { ItemHeader.IsLoading = value; }
        }

        [NotNull]
        public string Text
        {
            get { return ItemHeader.Text; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                ItemHeader.Text = value;
            }
        }

        [NotNull]
        protected BaseTreeViewItemHeader ItemHeader
        {
            get { return (BaseTreeViewItemHeader)Header; }
        }

        protected static bool Wait { get; set; }

        public virtual void Add([NotNull] object item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            RemoveDummy();
            Items.Add(item);
        }

        public virtual void Clear(bool makeExpandable = true)
        {
            foreach (var child in Items)
            {
                Notifications.RaiseUnloaded(this, child);
            }

            if (IsExpanded)
            {
                IsExpanded = false;
            }

            Items.Clear();

            if (makeExpandable)
            {
                MakeExpandable();
            }
        }

        public virtual void Collapse()
        {
            IsExpanded = false;

            foreach (var child in Items)
            {
                Notifications.RaiseUnloaded(this, child);
            }

            Items.Clear();
            MakeExpandable();
        }

        public virtual void ExpandAndWait()
        {
            Wait = true;

            try
            {
                IsExpanded = true;
            }
            finally
            {
                Wait = false;
            }
        }

        public abstract bool GetChildren([NotNull] GetChildrenDelegate callback, bool async);

        [CanBeNull]
        public ItemsControl GetParentTreeViewItem()
        {
            var parent = VisualTreeHelper.GetParent(this);

            while (parent != null && !(parent is TreeViewItem || parent is TreeView))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as ItemsControl;
        }

        public event MouseButtonEventHandler GutterClick;

        public event ContextMenuEventHandler GutterContextMenuOpening;

        public virtual void Insert(int index, [NotNull] object item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            RemoveDummy();
            Items.Insert(index, item);
        }

        public virtual void Insert([NotNull] object anchor, [NotNull] object item)
        {
            Assert.ArgumentNotNull(anchor, nameof(anchor));
            Assert.ArgumentNotNull(item, nameof(item));

            RemoveDummy();

            var index = Items.IndexOf(anchor);
            if (index < 0)
            {
                Items.Add(item);
                return;
            }

            Items.Insert(index, item);
        }

        public void MakeExpandable()
        {
            if (Items.Count > 0)
            {
                return;
            }

            Items.Add(DummyTreeViewItem.Instance);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var gutter = Template.FindName(@"PART_Gutter", this) as Image;
            if (gutter == null)
            {
                return;
            }

            gutter.MouseLeftButtonDown += RaiseGutterClick;
            gutter.ContextMenuOpening += RaiseGutterContextMenuOpening;
        }

        public virtual void Refresh()
        {
            foreach (var child in Items)
            {
                Notifications.RaiseUnloaded(this, child);
            }

            if (IsExpanded)
            {
                IsExpanded = false;
                Items.Clear();
                IsExpanded = true;
            }
            else
            {
                Items.Clear();
                MakeExpandable();
            }
        }

        public virtual void RefreshAndExpand(bool async = true)
        {
            try
            {
                IsExpanded = false;
            }
            catch (NullReferenceException)
            {
                // ignore weird error
                return;
            }

            foreach (var child in Items)
            {
                Notifications.RaiseUnloaded(this, child);
            }

            Items.Clear();

            if (async)
            {
                IsExpanded = true;
            }
            else
            {
                ExpandAndWait();
            }
        }

        public virtual void Remove()
        {
            var parent = Parent as ItemsControl;
            if (parent == null)
            {
                return;
            }

            Notifications.RaiseUnloaded(parent, this);
            parent.Items.Remove(this);
        }

        public virtual void Rename()
        {
            ItemHeader.Edit();
        }

        protected virtual void Expand(bool async)
        {
            var itemTreeView = GetItemTreeView();
            var filterText = itemTreeView == null ? string.Empty : itemTreeView.FilterText;

            IsLoading = true;

            GetChildrenDelegate completed = delegate(IEnumerable<BaseTreeViewItem> childItems)
            {
                var items = new List<BaseTreeViewItem>(childItems.Where(i => i != null));

                if (itemTreeView != null)
                {
                    if (itemTreeView.SupportsVirtualItems)
                    {
                        GetVirtualChildren(items);
                    }

                    itemTreeView.InternalFilterChildren(this, items);
                }

                RemoveDummy();
                foreach (var item in items)
                {
                    item.Visibility = item.Text.IsFilterMatch(filterText) ? Visibility.Visible : Visibility.Collapsed;
                    Items.Add(item);
                }

                IsLoading = false;

                UpdateGutter(items);
            };

            if (!GetChildren(completed, async))
            {
                IsLoading = false;
            }
        }

        [CanBeNull]
        protected ItemTreeView GetItemTreeView()
        {
            var element = this as FrameworkElement;

            while (element != null && !(element is ItemTreeView))
            {
                element = element.Parent as FrameworkElement;
            }

            return element as ItemTreeView;
        }

        protected virtual void GetVirtualChildren([NotNull] ICollection<BaseTreeViewItem> items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            GetChildrenPipeline.Run().WithParameters(items, this, null);
        }

        protected void Refresh([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Refresh();
        }

        protected virtual bool Renamed([NotNull] string newName)
        {
            Debug.ArgumentNotNull(newName, nameof(newName));

            return false;
        }

        protected virtual void UpdateGutter([NotNull] List<BaseTreeViewItem> items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            var gutterItems = new List<ItemUri>();

            foreach (var item in items)
            {
                var i = item as ItemTreeViewItem;
                if (i != null)
                {
                    gutterItems.Add(i.ItemUri);
                }
            }

            GutterManager.UpdateGutter(gutterItems);
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            var value = TryFindResource(@"BaseTreeViewItemStyle") as Style;
            if (value != null)
            {
                Style = value;
            }
        }

        private void Expand([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            RemoveDummy();
            if (Items.Count > 0)
            {
                return;
            }

            Expand(!Wait);
        }

        private void RaiseGutterClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var click = GutterClick;
            if (click != null)
            {
                click(sender, e);
            }
        }

        private void RaiseGutterContextMenuOpening([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var click = GutterContextMenuOpening;
            if (click != null)
            {
                click(sender, e);
            }
        }

        private void RemoveDummy()
        {
            if (Items.Count == 1 && Items[0] == DummyTreeViewItem.Instance)
            {
                Items.Clear();
            }
        }

        private void SetText([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Renamed(ItemHeader.Text);
        }
    }
}
