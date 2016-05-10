// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Shell.ComponentModel;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Items;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Models;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Overlays;
using Sitecore.Rocks.UI.LayoutDesigners.Properties;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Bindings
{
    public partial class BindingAnchor
    {
        public const string BindingAnchorDragIdentifier = "Sitecore.LayoutBuilder.Anchor";

        private OverlayConnector connector;

        private bool isDragging;

        private Point origin;

        public BindingAnchor([NotNull] PageModel pageModel, [NotNull] RenderingItem rendering, [NotNull] DynamicProperty dynamicProperty)
        {
            Assert.ArgumentNotNull(pageModel, nameof(pageModel));
            Assert.ArgumentNotNull(rendering, nameof(rendering));
            Assert.ArgumentNotNull(dynamicProperty, nameof(dynamicProperty));

            InitializeComponent();

            PageModel = pageModel;
            Rendering = rendering;
            DynamicProperty = dynamicProperty;
        }

        [NotNull]
        public DynamicProperty DynamicProperty { get; }

        [NotNull]
        public PageModel PageModel { get; private set; }

        [NotNull]
        public RenderingItem Rendering { get; }

        public void BindTo([NotNull] string name)
        {
            Assert.ArgumentNotNull(name, nameof(name));

            DynamicProperty.Value = "{Binding " + name + "}";
        }

        public void HandleDragOver([NotNull] RenderingTreeViewItem renderingTreeViewItem, [NotNull] DragEventArgs e)
        {
            Assert.ArgumentNotNull(renderingTreeViewItem, nameof(renderingTreeViewItem));
            Assert.ArgumentNotNull(e, nameof(e));

            if (renderingTreeViewItem.Rendering.DynamicProperties.Count == 0)
            {
                return;
            }

            if (renderingTreeViewItem.Rendering.RenderingContainer != Rendering.RenderingContainer)
            {
                return;
            }

            e.Effects = DragDropEffects.Link;
        }

        public void HandleDragOver([NotNull] PageModel renderingTreeViewItem, [NotNull] DragEventArgs e)
        {
            Assert.ArgumentNotNull(renderingTreeViewItem, nameof(renderingTreeViewItem));
            Assert.ArgumentNotNull(e, nameof(e));

            var subtype = DynamicProperty.Attributes["subtype"] as string ?? string.Empty;
            if (string.Compare(subtype, "click", StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                return;
            }

            e.Effects = DragDropEffects.Link;
            e.Handled = true;
        }

        public void HandleDrop([NotNull] RenderingTreeViewItem renderingTreeViewItem, [NotNull] DragEventArgs dragEventArgs)
        {
            Assert.ArgumentNotNull(renderingTreeViewItem, nameof(renderingTreeViewItem));
            Assert.ArgumentNotNull(dragEventArgs, nameof(dragEventArgs));

            var contextMenu = new ContextMenu
            {
                PlacementTarget = this
            };

            foreach (var dynamicProperty in renderingTreeViewItem.Rendering.DynamicProperties)
            {
                var bindMode = (BindingMode)dynamicProperty.Attributes["bindmode"];
                if (bindMode == BindingMode.Server || bindMode == BindingMode.Write)
                {
                    continue;
                }

                var property = dynamicProperty;

                var menuItem = new MenuItem
                {
                    Header = dynamicProperty.Name
                };

                menuItem.Click += (sender, args) => BindTo(renderingTreeViewItem.Rendering.GetDisplayName() + "." + property.Name);

                contextMenu.Items.Add(menuItem);
            }

            if (contextMenu.Items.Count > 0)
            {
                ContextMenu = contextMenu;
            }
        }

        public void HandleDrop([NotNull] PageModel pageModel, [NotNull] DragEventArgs dragEventArgs)
        {
            Assert.ArgumentNotNull(pageModel, nameof(pageModel));
            Assert.ArgumentNotNull(dragEventArgs, nameof(dragEventArgs));

            // this.DynamicProperty.Value = "javascript:window.location='/?sc_itemid=" + pageModel.DatabaseUri.ItemId + "'";
        }

        protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            base.OnGiveFeedback(e);

            if (connector != null)
            {
                var pos = PointFromScreen(AppHost.Shell.GetMousePosition());
                connector.SetPosition(TranslatePoint(pos, connector.OverlayCanvas));
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            base.OnMouseLeftButtonDown(e);

            DragManager.HandleMouseDown(this, e, out origin);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            base.OnMouseMove(e);

            if (isDragging)
            {
                return;
            }

            if (!DragManager.IsDragStart(this, e, ref origin))
            {
                return;
            }

            var layoutDesigner = this.GetAncestorOrSelf<LayoutDesigner>();
            if (layoutDesigner == null)
            {
                return;
            }

            ContextMenu = null;

            var pos = PointFromScreen(AppHost.Shell.GetMousePosition());

            var overlayCanvas = layoutDesigner.GetCanvas();
            overlayCanvas.StartDragging();
            connector = new OverlayConnector(overlayCanvas, TranslatePoint(pos, overlayCanvas));

            isDragging = true;

            var dragData = new DataObject(BindingAnchorDragIdentifier, this);

            DragManager.DoDragDrop(this, dragData, DragDropEffects.Link);

            isDragging = false;

            connector.Remove();
            connector = null;
            overlayCanvas.EndDragging();

            e.Handled = true;

            if (ContextMenu != null)
            {
                ContextMenu.IsOpen = true;
            }
        }
    }
}
