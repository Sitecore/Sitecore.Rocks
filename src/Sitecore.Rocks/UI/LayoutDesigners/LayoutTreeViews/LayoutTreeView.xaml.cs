// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.ContentTrees.Pipelines.DefaultActions;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.UI.LayoutDesigners.Controls;
using Sitecore.Rocks.UI.LayoutDesigners.Designers;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Items;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Models;
using Sitecore.Rocks.UI.LayoutDesigners.Properties.PlaceHolders;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews
{
    public partial class LayoutTreeView : ILayoutDesignerView, IContextProvider
    {
        public const string LayoutDesignerTreeviewDevices = "LayoutDesigner\\TreeView\\Devices";

        public const string LayoutDesignerTreeviewPlaceholders = "LayoutDesigner\\TreeView\\PlaceHolders";

        public const string LayoutDesignerTreeviewRenderings = "LayoutDesigner\\TreeView\\Renderings";

        private const string PropertySplitter = "LayoutDesigner\\PropertyViewSplitter";

        private static readonly EmptySelectionPane EmptySelectionPane = new EmptySelectionPane();

        public LayoutTreeView([NotNull] LayoutDesigner layoutDesigner)
        {
            Assert.ArgumentNotNull(layoutDesigner, nameof(layoutDesigner));

            InitializeComponent();

            LayoutDesigner = layoutDesigner;

            TreeView.GetContext = GetContext;
            TreeView.SelectedItemsChanged += RaiseSelectedItemsChanged;

            PropertyWindow.Content = EmptySelectionPane;
            PropertySplitterColumn.Width = new GridLength(AppHost.Settings.GetDouble(PropertySplitter, "Width", 200));
        }

        [NotNull]
        public ItemCollection Items => TreeView.Items;

        [NotNull]
        public LayoutDesigner LayoutDesigner { get; }

        [NotNull]
        public PageModel PageModel { get; private set; }

        [NotNull]
        public IEnumerable<BaseTreeViewItem> SelectedItems => TreeView.SelectedItems;

        public void AddPlaceholder(DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
        }

        public void AddRendering(RenderingItem rendering)
        {
            Assert.ArgumentNotNull(rendering, nameof(rendering));

            var treeViewIndex = -1;
            var renderingIndex = -1;

            var selectedItems = TreeView.SelectedItems;
            if (selectedItems.Count() != 1)
            {
                AppHost.MessageBox("Select a Placeholder first.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var placeHolderTreeViewItem = selectedItems.FirstOrDefault() as PlaceHolderTreeViewItem;
            if (placeHolderTreeViewItem == null)
            {
                var renderingTreeViewItem = selectedItems.FirstOrDefault() as RenderingTreeViewItem;
                if (renderingTreeViewItem != null)
                {
                    placeHolderTreeViewItem = renderingTreeViewItem.GetAncestor<PlaceHolderTreeViewItem>();
                    if (placeHolderTreeViewItem != null)
                    {
                        treeViewIndex = placeHolderTreeViewItem.Items.IndexOf(renderingTreeViewItem);
                        renderingIndex = placeHolderTreeViewItem.DeviceTreeViewItem.Device.Renderings.IndexOf(renderingTreeViewItem.Rendering);
                    }
                }
            }

            if (placeHolderTreeViewItem == null)
            {
                AppHost.MessageBox("Select a Placeholder first.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var deviceTreeViewItem = placeHolderTreeViewItem.GetAncestor<DeviceTreeViewItem>();
            if (deviceTreeViewItem == null)
            {
                return;
            }

            rendering.PlaceholderKey = new PlaceHolderKey(placeHolderTreeViewItem.PlaceHolderName);

            var newRenderingTreeViewItem = deviceTreeViewItem.AddRendering(placeHolderTreeViewItem, rendering, treeViewIndex, renderingIndex);

            newRenderingTreeViewItem.BringIntoView();
            newRenderingTreeViewItem.Focus();
            Keyboard.Focus(newRenderingTreeViewItem);
        }

        public void Clear()
        {
            Unload(Items);
            Items.Clear();
        }

        [NotNull]
        public object GetContext()
        {
            return GetContext(this);
        }

        [NotNull]
        public IEnumerable<object> GetSelectedObjects()
        {
            var selectedItems = SelectedItems.ToList();

            if (selectedItems.All(i => i is RenderingTreeViewItem))
            {
                return selectedItems.OfType<RenderingTreeViewItem>().Select(i => i.Rendering);
            }

            /*
      if (selectedItems.All(i => i is PlaceHolderTreeViewItem))
      {
        return selectedItems.OfType<PlaceHolderTreeViewItem>().Select(i => i.PlaceHolderName);
      }
      */
            return Enumerable.Empty<object>();
        }

        public void LoadLayout(DatabaseUri databaseUri, XElement layoutDefinition)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(layoutDefinition, nameof(layoutDefinition));

            PageModel = new PageModel(databaseUri, layoutDefinition);
            var pageTreeViewItem = new PageTreeViewItem(this, PageModel);

            Items.Add(pageTreeViewItem);

            PageModel.Modified += RaiseModified;
        }

        public void LocateInLayoutPane([NotNull] RenderingItem rendering)
        {
            Assert.ArgumentNotNull(rendering, nameof(rendering));

            var renderingTreeViewItem = FindItem<RenderingTreeViewItem>(i => i.Rendering == rendering);
            if (renderingTreeViewItem == null)
            {
                return;
            }

            var treeViewItem = renderingTreeViewItem.GetParentTreeViewItem() as BaseTreeViewItem;
            while (treeViewItem != null)
            {
                treeViewItem.IsExpanded = true;
                treeViewItem = treeViewItem.GetParentTreeViewItem() as BaseTreeViewItem;
            }

            renderingTreeViewItem.BringIntoView();

            renderingTreeViewItem.IsSelected = true;
            renderingTreeViewItem.Focus();
            Keyboard.Focus(renderingTreeViewItem);
        }

        public event EventHandler Modified;

        public void OpenMenu(object sender)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));

            var context = GetContext();

            var contextMenu = AppHost.ContextMenus.Build(context);
            if (contextMenu == null)
            {
                return;
            }

            contextMenu.Placement = PlacementMode.Bottom;
            contextMenu.PlacementTarget = sender as UIElement;
            contextMenu.IsOpen = true;
        }

        public void RaiseModified([NotNull] object sender, [NotNull] EventArgs eventArgs)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(eventArgs, nameof(eventArgs));

            var modified = Modified;
            if (modified != null)
            {
                modified(this, eventArgs);
            }

            LayoutDesigner.UpdateRibbon(this);
        }

        public void RemoveRendering(LayoutDesignerItem renderingItem)
        {
            Assert.ArgumentNotNull(renderingItem, nameof(renderingItem));

            var selectedItems = SelectedItems.ToList();
            RenderingTreeViewItem last = null;

            foreach (var rendering in selectedItems.OfType<RenderingTreeViewItem>())
            {
                rendering.RemoveRendering();
                last = rendering;
            }

            if (last != null)
            {
                last.DeviceTreeViewItem.Device.PageModel.RaiseModified();
            }
        }

        public void SaveLayout(XmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (PageModel == null)
            {
                return;
            }

            PageModel.Save(output, false);
        }

        public event RoutedPropertyChangedEventHandler<object> SelectedItemsChanged;

        public void UpdateTracking()
        {
            LayoutDesigner.UpdateRibbon(this);

            AppHost.Selection.Track(LayoutDesigner.Pane, GetSelectedObjects());

            UpdatePropertiesPane();
        }

        [CanBeNull]
        private T FindItem<T>([NotNull] Func<T, bool> predicate) where T : BaseTreeViewItem
        {
            Debug.ArgumentNotNull(predicate, nameof(predicate));

            return FindItem(TreeView.Items, predicate);
        }

        [CanBeNull]
        private T FindItem<T>([NotNull] ItemCollection items, [NotNull] Func<T, bool> predicate) where T : BaseTreeViewItem
        {
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(predicate, nameof(predicate));

            foreach (var baseTreeViewItem in items.OfType<BaseTreeViewItem>())
            {
                var t = baseTreeViewItem as T;
                if (t != null)
                {
                    if (predicate(t))
                    {
                        return baseTreeViewItem as T;
                    }
                }

                var result = FindItem(baseTreeViewItem.Items, predicate);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        [NotNull]
        private object GetContext([NotNull] object source)
        {
            Debug.ArgumentNotNull(source, nameof(source));

            var selectedItems = TreeView.GetSelectedItems(source);

            var items = Enumerable.Empty<LayoutDesignerItem>();

            if (selectedItems.All(i => i is RenderingTreeViewItem))
            {
                items = selectedItems.OfType<RenderingTreeViewItem>().Select(i => i.Rendering);
            }

            return new LayoutTreeViewContext(LayoutDesigner, selectedItems, items.FirstOrDefault(), items);
        }

        IRenderingContainer ILayoutDesignerView.GetRenderingContainer()
        {
            var selectedItems = TreeView.SelectedItems;
            if (selectedItems.Count() != 1)
            {
                return null;
            }

            var deviceTreeViewItem = selectedItems.First().GetAncestor<DeviceTreeViewItem>();
            if (deviceTreeViewItem == null)
            {
                return null;
            }

            return deviceTreeViewItem.Device;
        }

        private void HandleDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = GetContext(e.Source);

            var pipeline = DefaultActionPipeline.Run().WithParameters(context);

            e.Handled = pipeline.Handled;
        }

        private void RaiseSelectedItemsChanged([NotNull] object sender, [NotNull] RoutedPropertyChangedEventArgs<object> e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItemsChanged = SelectedItemsChanged;
            if (selectedItemsChanged != null)
            {
                selectedItemsChanged(sender, e);
            }

            LayoutDesigner.UpdateRibbon(this);
            UpdatePropertiesPane();
        }

        private void SavePropertyGridSplitter([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));
            Debug.ArgumentNotNull(sender, nameof(sender));

            AppHost.Settings.SetDouble(PropertySplitter, "Width", PropertySplitterColumn.Width.Value);
        }

        private void Unload([NotNull] ItemCollection items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            foreach (var item in items.OfType<BaseTreeViewItem>())
            {
                var appBuilderTreeViewItem = item as LayoutTreeViewItemBase;
                if (appBuilderTreeViewItem != null)
                {
                    appBuilderTreeViewItem.Unload();
                }

                Unload(item.Items);
            }
        }

        private void UpdatePropertiesPane()
        {
            var oldDesigner = PropertyWindow.Content as DesignerControl;
            if (oldDesigner != null)
            {
                oldDesigner.HandleClosed();
            }

            PropertyWindow.Content = EmptySelectionPane;

            var selectedItems = TreeView.SelectedItems;
            if (selectedItems.Count() != 1)
            {
                return;
            }

            object context = null;

            if (TreeView.SelectedItems.All(i => i is RenderingTreeViewItem))
            {
                var renderingTreeViewItem = selectedItems.OfType<RenderingTreeViewItem>().FirstOrDefault();
                if (renderingTreeViewItem == null)
                {
                    return;
                }

                context = new RenderingContext(renderingTreeViewItem);
            }

            if (TreeView.SelectedItems.All(i => i is PlaceHolderTreeViewItem))
            {
                var placeHolderTreeViewItem = selectedItems.OfType<PlaceHolderTreeViewItem>().FirstOrDefault();
                if (placeHolderTreeViewItem == null)
                {
                    return;
                }

                context = new PlaceHolderContext(placeHolderTreeViewItem);
            }

            if (TreeView.SelectedItems.All(i => i is PageTreeViewItem))
            {
                var pageTreeViewItem = selectedItems.OfType<PageTreeViewItem>().FirstOrDefault();
                if (pageTreeViewItem != null)
                {
                    context = new PageContext(pageTreeViewItem.PageModel);
                }
            }

            if (TreeView.SelectedItems.All(i => i is DeviceTreeViewItem))
            {
                var deviceTreeViewItem = selectedItems.OfType<DeviceTreeViewItem>().FirstOrDefault();
                if (deviceTreeViewItem != null)
                {
                    context = new DeviceContext(deviceTreeViewItem.Device);
                }
            }

            if (context == null)
            {
                return;
            }

            var renderingDesigner = new DesignerControl(context);

            PropertyWindow.Content = renderingDesigner;
        }
    }
}
