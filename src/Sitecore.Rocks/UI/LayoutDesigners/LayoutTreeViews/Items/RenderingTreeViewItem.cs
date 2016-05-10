// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Bindings;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Models;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Items
{
    public class RenderingTreeViewItem : LayoutTreeViewItemBase, IItem, ICanDrag, ICanDrop
    {
        private readonly ControlDragAdorner adorner;

        public RenderingTreeViewItem([NotNull] DeviceTreeViewItem deviceTreeViewItem, [NotNull] RenderingItem rendering)
        {
            Assert.ArgumentNotNull(deviceTreeViewItem, nameof(deviceTreeViewItem));
            Assert.ArgumentNotNull(rendering, nameof(rendering));

            DeviceTreeViewItem = deviceTreeViewItem;
            Rendering = rendering;
            Icon = rendering.Icon;
            Text = rendering.GetDisplayName() + " : " + rendering.Name;
            DataContext = this;

            Rendering.PropertyChanged += HandlePropertyChanged;

            adorner = new ControlDragAdorner(ItemHeader, ControlDragAdornerPosition.All);

            Loaded += ControlLoaded;
        }

        [NotNull]
        public DeviceTreeViewItem DeviceTreeViewItem { get; }

        public ItemUri ItemUri => Rendering.ItemUri;

        [NotNull]
        public RenderingItem Rendering { get; }

        [CanBeNull]
        protected Canvas BindingPane { get; set; }

        string IItem.Name => Rendering.Name;

        public override bool GetChildren(GetChildrenDelegate callback, bool async)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            callback(Enumerable.Empty<BaseTreeViewItem>());

            return true;
        }

        public string GetDragIdentifier()
        {
            return LayoutDesigner.DragIdentifier;
        }

        public void HandleDragOver(object sender, DragEventArgs e, BaseTreeViewItem treeViewItem)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));
            Assert.ArgumentNotNull(treeViewItem, nameof(treeViewItem));

            adorner.AllowedPositions = ControlDragAdornerPosition.None;

            if (e.Data.GetDataPresent(LayoutDesigner.DragIdentifier))
            {
                adorner.AllowedPositions = ControlDragAdornerPosition.Top | ControlDragAdornerPosition.Bottom;
                var h = adorner.GetHitTest(e);
                e.Effects = (h & adorner.AllowedPositions) != ControlDragAdornerPosition.None ? DragDropEffects.Move : DragDropEffects.None;

                /*
        if (e.Effects == DragDropEffects.Move && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
        {
          e.Effects = DragDropEffects.Copy;
        }
        */
            }
            else if (e.Data.GetDataPresent(DragManager.DragIdentifier))
            {
                adorner.AllowedPositions = ControlDragAdornerPosition.Top | ControlDragAdornerPosition.Bottom;
                var h = adorner.GetHitTest(e);
                e.Effects = (h & adorner.AllowedPositions) != ControlDragAdornerPosition.None ? DragDropEffects.Copy : DragDropEffects.None;
            }
            else if (e.Data.GetDataPresent(BindingAnchor.BindingAnchorDragIdentifier))
            {
                var anchor = (BindingAnchor)e.Data.GetData(BindingAnchor.BindingAnchorDragIdentifier);
                anchor.HandleDragOver(this, e);
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
            else if (e.Data.GetDataPresent(BindingAnchor.BindingAnchorDragIdentifier))
            {
                var anchor = (BindingAnchor)e.Data.GetData(BindingAnchor.BindingAnchorDragIdentifier);
                anchor.HandleDrop(this, e);
                e.Handled = true;
            }
        }

        public void RemoveRendering()
        {
            var pageModel = Rendering.RenderingContainer as DeviceModel;
            if (pageModel == null)
            {
                return;
            }

            pageModel.Delete(Rendering);

            var parent = GetParentTreeViewItem() as BaseTreeViewItem;

            Remove();

            if (parent is TempPlaceHolderTreeViewItem && parent.Items.Count == 0)
            {
                parent.Remove();
            }
        }

        public override void Unload()
        {
            Rendering.PropertyChanged -= HandlePropertyChanged;
        }

        protected override void OnCollapsed(RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            base.OnCollapsed(e);

            AppHost.Settings.SetBool(LayoutTreeView.LayoutDesignerTreeviewRenderings, Rendering.ItemId, false);
        }

        protected override void OnExpanded(RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            base.OnExpanded(e);

            AppHost.Settings.SetBool(LayoutTreeView.LayoutDesignerTreeviewRenderings, Rendering.ItemId, true);
        }

        protected override bool Renamed(string newName)
        {
            Debug.ArgumentNotNull(newName, nameof(newName));

            base.Renamed(newName);

            Rendering.SetParameterValue("iD", newName);

            return true;
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            var value = TryFindResource(@"RenderingTreeViewItem") as Style;
            if (value != null)
            {
                Style = value;
            }
        }

        private void CopyRenderings([NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            var placeHolderTreeViewItem = GetParentTreeViewItem() as PlaceHolderTreeViewItem;
            if (placeHolderTreeViewItem == null)
            {
                return;
            }

            var treeViewIndex = placeHolderTreeViewItem.Items.IndexOf(this);
            var renderingIndex = DeviceTreeViewItem.Device.Renderings.IndexOf(Rendering);

            if (adorner.LastPosition == ControlDragAdornerPosition.Bottom)
            {
                treeViewIndex++;
                renderingIndex++;
            }

            var items = (IEnumerable<BaseTreeViewItem>)e.Data.GetData(LayoutDesigner.DragIdentifier);

            foreach (var item in items.OfType<RenderingTreeViewItem>())
            {
                var renderingTreeViewItem = DeviceTreeViewItem.AddRendering(placeHolderTreeViewItem, item, treeViewIndex, renderingIndex);
                renderingTreeViewItem.Rendering.CopyFrom(item.Rendering);
                renderingTreeViewItem.Rendering.PlaceholderKey.Key = placeHolderTreeViewItem.PlaceHolderName;
            }
        }

        private void DropItems([NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            var placeHolderTreeViewItem = GetParentTreeViewItem() as PlaceHolderTreeViewItem;
            if (placeHolderTreeViewItem == null)
            {
                return;
            }

            var treeViewIndex = placeHolderTreeViewItem.Items.IndexOf(this);
            var renderingIndex = DeviceTreeViewItem.Device.Renderings.IndexOf(Rendering);
            if (adorner.LastPosition == ControlDragAdornerPosition.Bottom)
            {
                treeViewIndex++;
                renderingIndex++;
            }

            var items = (IEnumerable<IItem>)e.Data.GetData(DragManager.DragIdentifier);

            RenderingTreeViewItem rendering = null;
            foreach (var item in items)
            {
                var r = DeviceTreeViewItem.AddRendering(placeHolderTreeViewItem, item, treeViewIndex, renderingIndex);

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
            var items = draggedItems.OfType<RenderingTreeViewItem>().ToList();

            if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift)
            {
                string text;
                if (items.Count == 1)
                {
                    text = "Are you sure you want to move this rendering?";
                }
                else
                {
                    text = string.Format("Are you sure you want to move these {0} renderings?", items.Count);
                }

                if (AppHost.MessageBox(text, "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                {
                    return;
                }
            }

            var renderings = DeviceTreeViewItem.Device.Renderings;

            foreach (var item in items)
            {
                item.Remove();
                renderings.Remove(item.Rendering);
            }

            ItemCollection treeViewItems;
            int treeViewIndex;
            int renderingIndex;

            if (adorner.LastPosition == ControlDragAdornerPosition.Over)
            {
                treeViewItems = Items;
                treeViewIndex = treeViewItems.Count;
                renderingIndex = renderings.Count;
            }
            else
            {
                var parentTreeViewItem = GetParentTreeViewItem();
                if (parentTreeViewItem == null)
                {
                    return;
                }

                treeViewItems = parentTreeViewItem.Items;
                treeViewIndex = treeViewItems.IndexOf(this);
                renderingIndex = renderings.IndexOf(Rendering);

                if (adorner.LastPosition == ControlDragAdornerPosition.Bottom)
                {
                    treeViewIndex++;
                    renderingIndex++;
                }
            }

            var first = true;
            for (var i = items.Count - 1; i >= 0; i--)
            {
                var renderingTreeViewItem = items[i];

                treeViewItems.Insert(treeViewIndex, renderingTreeViewItem);
                renderings.Insert(renderingIndex, renderingTreeViewItem.Rendering);

                renderingTreeViewItem.Rendering.PlaceholderKey.Key = Rendering.PlaceholderKey.Key;
                if (first)
                {
                    renderingTreeViewItem.IsSelected = true;
                    renderingTreeViewItem.Focus();
                    Keyboard.Focus(renderingTreeViewItem);
                }

                first = false;
            }

            DeviceTreeViewItem.Device.PageModel.RaiseModified();
        }

        private void HandlePropertyChanged([NotNull] object sender, [NotNull] PropertyChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.PropertyName == "PlaceholderKey")
            {
                TrackPlaceHolder();
            }

            if (e.PropertyName == "Name")
            {
                Text = Rendering.GetDisplayName() + " : " + Rendering.Name;
            }

            if (e.PropertyName == "Icon")
            {
                Icon = Rendering.Icon;
            }

            if (e.PropertyName == "Id")
            {
                Text = Rendering.GetDisplayName() + " : " + Rendering.Name;

                var controlId = Rendering.GetControlId();

                // TODO: this should be more general
                var placeHolders = Items.OfType<PlaceHolderTreeViewItem>();
                foreach (var item in placeHolders)
                {
                    var name = item.PlaceHolderName;
                    var n = name.IndexOf('.');
                    if (n >= 0)
                    {
                        item.PlaceHolderName = controlId + name.Mid(n);
                        item.Text = item.PlaceHolderName + " : Placeholder";
                    }
                }
            }

            DeviceTreeViewItem.Device.PageModel.RaiseModified();
        }

        private void TrackPlaceHolder()
        {
            var placeHolderName = Rendering.PlaceholderKey.ToString();
            var placeHolderTreeViewItem = DeviceTreeViewItem.FindPlaceHolderTreeViewItem(placeHolderName) as BaseTreeViewItem;

            if (placeHolderTreeViewItem == null)
            {
                placeHolderTreeViewItem = new TempPlaceHolderTreeViewItem(DeviceTreeViewItem, placeHolderName)
                {
                    IsExpanded = true
                };

                DeviceTreeViewItem.Items.Add(placeHolderTreeViewItem);
            }

            var parent = GetParentTreeViewItem() as BaseTreeViewItem;

            Remove();

            if (parent is TempPlaceHolderTreeViewItem && parent.Items.Count == 0)
            {
                parent.Remove();
            }

            placeHolderTreeViewItem.Items.Add(this);

            placeHolderTreeViewItem.IsSelected = true;
        }
    }
}
