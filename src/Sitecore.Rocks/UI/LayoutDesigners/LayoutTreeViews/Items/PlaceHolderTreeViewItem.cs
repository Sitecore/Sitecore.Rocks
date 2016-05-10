// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Items
{
    public class PlaceHolderTreeViewItem : LayoutTreeViewItemBase, ICanDrop
    {
        private readonly ControlDragAdorner _adorner;

        public PlaceHolderTreeViewItem([NotNull] DeviceTreeViewItem deviceTreeViewItem, [NotNull] string placeHolderName)
        {
            Assert.ArgumentNotNull(deviceTreeViewItem, nameof(deviceTreeViewItem));
            Assert.ArgumentNotNull(placeHolderName, nameof(placeHolderName));

            DeviceTreeViewItem = deviceTreeViewItem;
            PlaceHolderName = placeHolderName;
            Text = placeHolderName + " : Placeholder";
            Icon = new Icon("Resources/16x16/bullet_square_grey.png");
            DataContext = this;

            _adorner = new ControlDragAdorner(ItemHeader, ControlDragAdornerPosition.All);

            Loaded += ControlLoaded;
        }

        [NotNull]
        public DeviceTreeViewItem DeviceTreeViewItem { get; }

        [NotNull]
        public string PlaceHolderName { get; internal set; }

        public override bool GetChildren(GetChildrenDelegate callback, bool async)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            callback(Enumerable.Empty<BaseTreeViewItem>());

            return true;
        }

        [NotNull]
        public string GetPlaceHolderPath()
        {
            var result = string.Empty;
            var item = this;

            do
            {
                result = "/" + item.PlaceHolderName + result;
                item = item.GetAncestor<PlaceHolderTreeViewItem>();
            }
            while (item != null);

            return result;
        }

        public void HandleDragOver(object sender, DragEventArgs e, BaseTreeViewItem treeViewItem)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));
            Assert.ArgumentNotNull(treeViewItem, nameof(treeViewItem));

            _adorner.AllowedPositions = ControlDragAdornerPosition.None;

            if (e.Data.GetDataPresent(LayoutDesigner.DragIdentifier))
            {
                // e.Effects = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) ? DragDropEffects.Copy : DragDropEffects.Move;
                e.Effects = DragDropEffects.Move;
                _adorner.AllowedPositions = ControlDragAdornerPosition.Over;
            }
            else if (e.Data.GetDataPresent(DragManager.DragIdentifier))
            {
                e.Effects = DragDropEffects.Copy;
                _adorner.AllowedPositions = ControlDragAdornerPosition.Over;
            }
        }

        public void HandleDrop(object sender, DragEventArgs e, BaseTreeViewItem treeViewItem)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));
            Assert.ArgumentNotNull(treeViewItem, nameof(treeViewItem));

            if (e.Data.GetDataPresent(LayoutDesigner.DragIdentifier))
            {
                /*
        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
        {
          this.CopyRenderings(e);
        }
        else
        {
          this.DropRenderings(e);
        }
        */

                DropRenderings(e);

                e.Handled = true;
            }
            else if (e.Data.GetDataPresent(DragManager.DragIdentifier))
            {
                DropItems(e);
                e.Handled = true;
            }
        }

        public override void Unload()
        {
        }

        protected override void OnCollapsed(RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            base.OnCollapsed(e);
            AppHost.Settings.SetBool(LayoutTreeView.LayoutDesignerTreeviewPlaceholders, Text, false);
        }

        protected override void OnExpanded(RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            base.OnExpanded(e);

            AppHost.Settings.SetBool(LayoutTreeView.LayoutDesignerTreeviewPlaceholders, Text, true);
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            var value = TryFindResource(@"PlaceHolderTreeViewItem") as Style;
            if (value != null)
            {
                Style = value;
            }
        }

        private void CopyRenderings([NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            var items = (IEnumerable<BaseTreeViewItem>)e.Data.GetData(LayoutDesigner.DragIdentifier);

            foreach (var item in items.OfType<RenderingTreeViewItem>())
            {
                var renderingTreeViewItem = DeviceTreeViewItem.AddRendering(this, item.Rendering, -1, -1);
                renderingTreeViewItem.Rendering.CopyFrom(item.Rendering);
                renderingTreeViewItem.Rendering.PlaceholderKey.Key = PlaceHolderName;
            }
        }

        private void DropItems([NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            var items = (IEnumerable<IItem>)e.Data.GetData(DragManager.DragIdentifier);

            RenderingTreeViewItem rendering = null;
            foreach (var item in items)
            {
                var r = DeviceTreeViewItem.AddRendering(this, item, -1, -1);

                if (rendering == null)
                {
                    rendering = r;
                }
            }

            if (rendering == null)
            {
                return;
            }

            rendering.BringIntoView();
            rendering.Focus();
            Keyboard.Focus(rendering);
        }

        private void DropRenderings([NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            var draggedItems = (List<BaseTreeViewItem>)e.Data.GetData(LayoutDesigner.DragIdentifier);
            var items = new List<BaseTreeViewItem>(draggedItems);

            if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift)
            {
                string text;
                if (items.Count == 1)
                {
                    text = "Are you sure you want to move this rendering?";
                }
                else
                {
                    text = $"Are you sure you want to move these {items.Count} renderings?";
                }

                if (AppHost.MessageBox(text, "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                {
                    return;
                }
            }

            foreach (var item in items)
            {
                var parent = GetParentTreeViewItem() as BaseTreeViewItem;

                item.Remove();

                if (parent is TempPlaceHolderTreeViewItem && parent.Items.Count == 0)
                {
                    parent.Remove();
                }
            }

            foreach (var item in new List<BaseTreeViewItem>(items))
            {
                Items.Add(item);

                var rendering = item as RenderingTreeViewItem;
                if (rendering != null)
                {
                    rendering.Rendering.PlaceholderKey.Key = PlaceHolderName;
                }
            }

            DeviceTreeViewItem.Device.PageModel.RaiseModified();
        }
    }
}
